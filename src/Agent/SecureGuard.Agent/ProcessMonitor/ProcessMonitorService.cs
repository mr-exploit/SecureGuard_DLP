using System.Management;
using Microsoft.Extensions.Options;
using SecureGuard.Agent.Alert;
using SecureGuard.Agent.Detection.Models;
using SecureGuard.Agent.Logging;
using SecureGuard.Shared.Constants;

namespace SecureGuard.Agent.ProcessMonitor;

public class ProcessMonitorService
{
    private readonly ILogger<ProcessMonitorService> _logger;
    private readonly AgentOptions _options;
    private readonly AlertService _alertService;
    private readonly RemoteLogService _logService;
    private ManagementEventWatcher? _processWatcher;
    private ManagementEventWatcher? _networkWatcher;
    private bool _running;

    private static readonly string[] SensitiveFilePatterns =
    {
        ".env", "secrets.json", "credentials.json", "app.config",
        "id_rsa", "id_ed25519", ".pem", ".key", ".pfx"
    };

    public ProcessMonitorService(
        ILogger<ProcessMonitorService> logger,
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
        if (!_options.ProcessMonitorEnabled)
        {
            _logger.LogInformation("Process monitoring is disabled");
            return;
        }

        try
        {
            StartProcessWatcher();
            _running = true;
            _logger.LogInformation("Process monitoring started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start process monitoring");
        }
    }

    public void Stop()
    {
        _running = false;
        _processWatcher?.Stop();
        _processWatcher?.Dispose();
        _networkWatcher?.Stop();
        _networkWatcher?.Dispose();
        _logger.LogInformation("Process monitoring stopped");
    }

    private void StartProcessWatcher()
    {
        try
        {
            // WMI query to watch for process creation
            var query = new WqlEventQuery(
                "SELECT * FROM Win32_ProcessStartTrace");

            _processWatcher = new ManagementEventWatcher(query);
            _processWatcher.EventArrived += OnProcessCreated;
            _processWatcher.Start();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WMI process watcher not available (may require elevated privileges)");
        }
    }

    private async void OnProcessCreated(object sender, EventArrivedEventArgs e)
    {
        if (!_running) return;

        try
        {
            var processName = e.NewEvent["ProcessName"]?.ToString() ?? "Unknown";
            var pid = Convert.ToInt32(e.NewEvent["ProcessID"]);

            _logger.LogDebug("Process started: {Name} (PID: {Pid})", processName, pid);

            // Check if process is accessing sensitive files via command line
            var commandLine = e.NewEvent["CommandLine"]?.ToString() ?? "";
            if (IsSensitiveCommandLine(commandLine))
            {
                var result = new DetectionResult
                {
                    IsViolation = true,
                    ViolationType = ViolationTypes.SensitiveFileAccess,
                    Severity = SeverityLevels.High,
                    Action = "Alert",
                    Message = $"Proses mengakses file sensitif: {processName}",
                    Details = $"Process: {processName} (PID: {pid}) Command: {commandLine}",
                    ProcessName = processName
                };

                await _alertService.ShowAlertAsync(result);
                await _logService.SendLogAsync(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WMI event");
        }
    }

    private static bool IsSensitiveCommandLine(string commandLine)
    {
        if (string.IsNullOrEmpty(commandLine)) return false;
        foreach (var pattern in SensitiveFilePatterns)
        {
            if (commandLine.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
