using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using SecureGuard.Agent.Alert;
using SecureGuard.Agent.Detection.Models;
using SecureGuard.Agent.Logging;
using SecureGuard.Shared.Constants;

namespace SecureGuard.Agent.FileMonitor;

public class FileWatcherService
{
    private readonly List<FileSystemWatcher> _watchers = new();
    private readonly ILogger<FileWatcherService> _logger;
    private readonly AgentOptions _options;
    private readonly AlertService _alertService;
    private readonly RemoteLogService _logService;

    private static readonly string[] SensitivePatterns =
    {
        "*.env",
        "app.config",
        "secrets.json",
        "credentials.json",
        "web.config",
        "*.pem",
        "*.key",
        "*.pfx"
    };

    private static readonly Regex[] CredentialRegexes =
    {
        new(@"AWS_SECRET_ACCESS_KEY\s*[=:]\s*[A-Za-z0-9+/]{40}", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"AKIA[0-9A-Z]{16}", RegexOptions.Compiled),
        new(@"API_KEY\s*[=:]\s*[A-Za-z0-9\-_]{16,}", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"DATABASE_PASSWORD\s*[=:]\s*\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"JWT_SECRET\s*[=:]\s*\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"password\s*=\s*[^\s&]{6,}", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"secret\s*=\s*[^\s&]{6,}", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"-----BEGIN (RSA |EC )?PRIVATE KEY-----", RegexOptions.Compiled)
    };

    public FileWatcherService(
        ILogger<FileWatcherService> logger,
        IOptions<AgentOptions> options,
        AlertService alertService,
        RemoteLogService logService)
    {
        _logger = logger;
        _options = options.Value;
        _alertService = alertService;
        _logService = logService;
    }

    public void Start()
    {
        if (!_options.FileMonitorEnabled)
        {
            _logger.LogInformation("File monitoring is disabled");
            return;
        }

        foreach (var path in _options.MonitoredPaths)
        {
            if (!Directory.Exists(path))
            {
                _logger.LogWarning("Monitored path does not exist: {Path}", path);
                continue;
            }

            foreach (var pattern in SensitivePatterns)
            {
                try
                {
                    var watcher = new FileSystemWatcher(path, pattern)
                    {
                        NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite |
                                       NotifyFilters.FileName | NotifyFilters.CreationTime,
                        IncludeSubdirectories = true,
                        EnableRaisingEvents = true
                    };

                    watcher.Created += OnFileEvent;
                    watcher.Changed += OnFileEvent;
                    watcher.Renamed += OnFileRenamed;

                    _watchers.Add(watcher);
                    _logger.LogInformation("Watching {Path} for pattern {Pattern}", path, pattern);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create watcher for {Path}/{Pattern}", path, pattern);
                }
            }
        }

        _logger.LogInformation("File monitoring started with {Count} watchers", _watchers.Count);
    }

    public void Stop()
    {
        foreach (var watcher in _watchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
        _watchers.Clear();
        _logger.LogInformation("File monitoring stopped");
    }

    private async void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        await ProcessFileAsync(e.FullPath, e.ChangeType.ToString());
    }

    private async void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        await ProcessFileAsync(e.FullPath, "Renamed");
    }

    private async Task ProcessFileAsync(string filePath, string changeType)
    {
        try
        {
            _logger.LogDebug("Sensitive file {ChangeType}: {FilePath}", changeType, filePath);

            // Try to read the file and scan for credentials
            if (!File.Exists(filePath)) return;

            await Task.Delay(100); // Brief delay to let file be written

            string content;
            try
            {
                content = await File.ReadAllTextAsync(filePath);
            }
            catch (IOException)
            {
                return; // File may be locked
            }

            var matchedPattern = "";
            foreach (var regex in CredentialRegexes)
            {
                var match = regex.Match(content);
                if (match.Success)
                {
                    matchedPattern = regex.ToString();
                    break;
                }
            }

            var result = new DetectionResult
            {
                IsViolation = true,
                ViolationType = string.IsNullOrEmpty(matchedPattern)
                    ? ViolationTypes.SensitiveFileAccess
                    : ViolationTypes.CredentialFile,
                Severity = string.IsNullOrEmpty(matchedPattern)
                    ? SeverityLevels.Medium
                    : SeverityLevels.Critical,
                Action = "Alert",
                Message = string.IsNullOrEmpty(matchedPattern)
                    ? $"Akses ke file sensitif terdeteksi: {Path.GetFileName(filePath)}"
                    : $"File credential terdeteksi: {Path.GetFileName(filePath)}",
                Details = $"File {changeType.ToLower()}: {filePath}" +
                          (string.IsNullOrEmpty(matchedPattern) ? "" : $" | Pattern: {matchedPattern}"),
                FilePath = filePath
            };

            await _alertService.ShowAlertAsync(result);
            await _logService.SendLogAsync(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file event for {FilePath}", filePath);
        }
    }
}
