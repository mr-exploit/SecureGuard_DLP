using Microsoft.Extensions.Options;
using SecureGuard.Agent.Alert;
using SecureGuard.Agent.Detection;
using SecureGuard.Agent.Detection.Models;
using SecureGuard.Agent.Logging;
using SecureGuard.Shared.Constants;
using System.Management;

namespace SecureGuard.Agent.ProcessMonitor;

public class ProcessMonitorService
{
    private readonly ILogger<ProcessMonitorService> _logger;
    private readonly AgentOptions _options;
    private readonly DetectionEngine _detectionEngine;
    private readonly AlertService _alertService;
    private readonly RemoteLogService _logService;
    private ManagementEventWatcher? _processWatcher;
    private readonly CancellationTokenSource _cts = new();
    private Task? _monitorTask;

    public ProcessMonitorService(
        ILogger<ProcessMonitorService> logger,
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
        try
        {
            StartWmiWatcher();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WMI watcher not available. Process monitoring limited.");
        }
        _monitorTask = MonitorActiveProcessesAsync(_cts.Token);
        _logger.LogInformation("ProcessMonitorService started.");
    }

    public void Stop()
    {
        _cts.Cancel();
        _processWatcher?.Stop();
        _processWatcher?.Dispose();
        _logger.LogInformation("ProcessMonitorService stopped.");
    }

    private void StartWmiWatcher()
    {
        var query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
        _processWatcher = new ManagementEventWatcher(query);
        _processWatcher.EventArrived += OnProcessStarted;
        _processWatcher.Start();
    }

    private void OnProcessStarted(object sender, EventArrivedEventArgs e)
    {
        try
        {
            var processName = e.NewEvent.Properties["ProcessName"]?.Value?.ToString() ?? "unknown";
            var pid = Convert.ToInt32(e.NewEvent.Properties["ProcessID"]?.Value ?? 0);
            _logger.LogDebug("Process started: {Name} (PID: {Pid})", processName, pid);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing WMI event.");
        }
    }

    private async Task MonitorActiveProcessesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await CheckActiveNetworkConnectionsAsync();
                await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in process monitoring loop.");
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }
    }

    private async Task CheckActiveNetworkConnectionsAsync()
    {
        try
        {
            var processes = System.Diagnostics.Process.GetProcesses();
            foreach (var process in processes)
            {
                try
                {
                    // Check processes that might be exfiltrating data
                    if (IsSuspiciousProcess(process.ProcessName))
                    {
                        _logger.LogDebug("Monitoring process: {Name} (PID: {Id})", process.ProcessName, process.Id);
                    }
                }
                catch
                {
                    // Process may have exited
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking network connections.");
        }
        await Task.CompletedTask;
    }

    private static bool IsSuspiciousProcess(string processName)
    {
        var suspicious = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ftp", "sftp", "scp", "winscp", "filezilla", "dropbox",
            "googledrive", "onedrive", "curl", "wget", "powershell"
        };
        return suspicious.Contains(processName);
    }

    public async Task<DetectionResult> EvaluateProcessAsync(string processName, int pid, string destinationIp)
    {
        var context = new Detection.Rules.DetectionContext
        {
            ProcessName = processName,
            ProcessId = pid,
            DestinationIp = destinationIp
        };

        return await _detectionEngine.CheckUnknownIpAsync(destinationIp);
    }
}
