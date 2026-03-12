using Microsoft.Extensions.Options;
using SecureGuard.Agent.Alert;
using SecureGuard.Agent.Detection;
using SecureGuard.Agent.Logging;

namespace SecureGuard.Agent.FileMonitor;

public class FileWatcherService
{
    private readonly ILogger<FileWatcherService> _logger;
    private readonly AgentOptions _options;
    private readonly DetectionEngine _detectionEngine;
    private readonly AlertService _alertService;
    private readonly RemoteLogService _logService;
    private readonly List<FileSystemWatcher> _watchers = new();

    private static readonly string[] SensitivePatterns =
    {
        ".env",
        "*.env",
        "app.config",
        "secrets.json",
        "credentials.json",
        "web.config",
        "*.pfx",
        "*.pem",
        "*.key",
        "appsettings*.json"
    };

    public FileWatcherService(
        ILogger<FileWatcherService> logger,
        IOptions<AgentOptions> options,
        DetectionEngine detectionEngine,
        AlertService alertService,
        RemoteLogService logService)
    {
        _logger = logger;
        _options = options.Value;
        _detectionEngine = detectionEngine;
        _alertService = alertService;
        _logService = logService;
    }

    public void Start()
    {
        foreach (var watchPath in _options.WatchPaths)
        {
            if (!Directory.Exists(watchPath))
            {
                _logger.LogWarning("Watch path does not exist: {Path}", watchPath);
                continue;
            }

            foreach (var pattern in SensitivePatterns)
            {
                var watcher = new FileSystemWatcher(watchPath, pattern)
                {
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName
                };

                watcher.Created += OnFileEvent;
                watcher.Changed += OnFileEvent;
                watcher.Renamed += OnFileRenamed;

                _watchers.Add(watcher);
            }

            _logger.LogInformation("FileWatcher started for path: {Path}", watchPath);
        }
    }

    public void Stop()
    {
        foreach (var watcher in _watchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
        _watchers.Clear();
        _logger.LogInformation("FileWatcher stopped.");
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        _ = HandleFileEventAsync(e.FullPath);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        _ = HandleFileEventAsync(e.FullPath);
    }

    private async Task HandleFileEventAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Sensitive file event: {FilePath}", filePath);

            string content = string.Empty;
            try
            {
                await Task.Delay(100); // Brief delay to let file write complete
                content = await File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read file: {FilePath}", filePath);
            }

            var result = await _detectionEngine.ScanFileContentAsync(filePath, content);

            if (result.IsBlocked || result.IsFlagged)
            {
                _logger.LogWarning("Violation in file {FilePath}: {Message}", filePath, result.Message);
                await _alertService.ShowAlertAsync(result.Message, result);
                await _logService.SendLogAsync(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling file event for {FilePath}", filePath);
        }
    }
}
