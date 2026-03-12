using SecureGuard.Agent.Alert;
using SecureGuard.Agent.FileMonitor;
using SecureGuard.Agent.Logging;
using SecureGuard.Agent.ProcessMonitor;
using SecureGuard.Agent.Proxy;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "SecureGuard Agent";
});

builder.Services.Configure<AgentOptions>(builder.Configuration.GetSection("Agent"));

builder.Services.AddSingleton<RemoteLogService>();
builder.Services.AddSingleton<AlertService>();
builder.Services.AddSingleton<ProxyService>();
builder.Services.AddSingleton<FileWatcherService>();
builder.Services.AddSingleton<ProcessMonitorService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
