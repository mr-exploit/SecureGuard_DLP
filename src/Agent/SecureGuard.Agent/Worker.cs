using SecureGuard.Agent.Alert;
using SecureGuard.Agent.FileMonitor;
using SecureGuard.Agent.Logging;
using SecureGuard.Agent.ProcessMonitor;
using SecureGuard.Agent.Proxy;

namespace SecureGuard.Agent;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ProxyService _proxyService;
    private readonly FileWatcherService _fileWatcherService;
    private readonly ProcessMonitorService _processMonitorService;
    private readonly RemoteLogService _remoteLogService;
    private readonly AlertService _alertService;

    public Worker(
        ILogger<Worker> logger,
        ProxyService proxyService,
        FileWatcherService fileWatcherService,
        ProcessMonitorService processMonitorService,
        RemoteLogService remoteLogService,
        AlertService alertService)
    {
        _logger = logger;
        _proxyService = proxyService;
        _fileWatcherService = fileWatcherService;
        _processMonitorService = processMonitorService;
        _remoteLogService = remoteLogService;
        _alertService = alertService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SecureGuard Agent starting...");

        try
        {
            await _proxyService.StartAsync(stoppingToken);
            _fileWatcherService.Start();
            _processMonitorService.Start();

            _logger.LogInformation("SecureGuard Agent started successfully");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                _logger.LogDebug("SecureGuard Agent heartbeat");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SecureGuard Agent stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SecureGuard Agent encountered a fatal error");
            throw;
        }
        finally
        {
            _proxyService.Stop();
            _fileWatcherService.Stop();
            _processMonitorService.Stop();
            _logger.LogInformation("SecureGuard Agent stopped");
        }
    }
}
