using SecureGuard.Agent;
using SecureGuard.Agent.Alert;
using SecureGuard.Agent.Detection;
using SecureGuard.Agent.FileMonitor;
using SecureGuard.Agent.Logging;
using SecureGuard.Agent.ProcessMonitor;
using SecureGuard.Agent.Proxy;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "SecureGuard Agent";
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<AgentOptions>(context.Configuration.GetSection("Agent"));
        services.AddSingleton<SslCertManager>();
        services.AddSingleton<RemoteLogService>();
        services.AddSingleton<AlertService>();
        services.AddSingleton<DetectionEngine>();
        services.AddSingleton<ProxyService>();
        services.AddSingleton<FileWatcherService>();
        services.AddSingleton<ProcessMonitorService>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
