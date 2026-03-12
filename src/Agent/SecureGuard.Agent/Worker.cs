using Microsoft.Extensions.Options;
using SecureGuard.Agent.FileMonitor;
using SecureGuard.Agent.ProcessMonitor;
using SecureGuard.Agent.Proxy;

namespace SecureGuard.Agent;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ProxyService _proxyService;
    private readonly FileWatcherService _fileWatcherService;
    private readonly ProcessMonitorService _processMonitorService;
    private readonly AgentOptions _options;

    public Worker(
        ILogger<Worker> logger,
        ProxyService proxyService,
        FileWatcherService fileWatcherService,
        ProcessMonitorService processMonitorService,
        IOptions<AgentOptions> options)
    {
        _logger = logger;
        _proxyService = proxyService;
        _fileWatcherService = fileWatcherService;
        _processMonitorService = processMonitorService;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SecureGuard Agent starting. AgentId={AgentId}", _options.AgentId);

        try
        {
            await _proxyService.StartAsync(stoppingToken);
            _fileWatcherService.Start();
            _processMonitorService.Start();

            _logger.LogInformation("SecureGuard Agent started. Proxy listening on {Host}:{Port}",
                _options.ProxyHost, _options.ProxyPort);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                _logger.LogDebug("SecureGuard Agent heartbeat.");
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
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
            _logger.LogInformation("SecureGuard Agent stopped.");
        }
    }
}
