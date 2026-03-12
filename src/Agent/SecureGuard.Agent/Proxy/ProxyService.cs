using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using SecureGuard.Agent.Alert;
using SecureGuard.Agent.Detection;
using SecureGuard.Agent.Logging;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace SecureGuard.Agent.Proxy;

public class ProxyService
{
    private readonly ProxyServer _proxyServer;
    private readonly CertificateManager _certManager;
    private readonly ILogger<ProxyService> _logger;
    private readonly AgentOptions _options;
    private DetectionEngine? _detectionEngine;
    private readonly AlertService _alertService;
    private readonly RemoteLogService _logService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IOptions<AgentOptions> _agentOptions;

    public ProxyService(
        ILogger<ProxyService> logger,
        IOptions<AgentOptions> options,
        AlertService alertService,
        RemoteLogService logService,
        ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _options = options.Value;
        _alertService = alertService;
        _logService = logService;
        _loggerFactory = loggerFactory;
        _agentOptions = options;

        _proxyServer = new ProxyServer();
        _certManager = new CertificateManager(_proxyServer, _loggerFactory.CreateLogger<CertificateManager>());
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.ProxyEnabled)
        {
            _logger.LogInformation("Proxy is disabled, skipping start");
            return;
        }

        try
        {
            _detectionEngine = new DetectionEngine(
                _alertService,
                _logService,
                _loggerFactory.CreateLogger<DetectionEngine>(),
                _agentOptions);

            _certManager.EnsureRootCa();

            _proxyServer.BeforeRequest += OnRequestAsync;
            _proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;

            var endpoint = new ExplicitProxyEndPoint(
                IPAddress.Parse("127.0.0.1"),
                _options.ProxyPort,
                decryptSsl: true);

            _proxyServer.AddEndPoint(endpoint);
            _proxyServer.Start();

            _logger.LogInformation("Proxy started on 127.0.0.1:{Port}", _options.ProxyPort);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start proxy server");
        }
    }

    public void Stop()
    {
        try
        {
            _proxyServer.BeforeRequest -= OnRequestAsync;
            if (_proxyServer.ProxyRunning)
            {
                _proxyServer.Stop();
            }
            _proxyServer.Dispose();
            _logger.LogInformation("Proxy stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping proxy");
        }
    }

    private async Task OnRequestAsync(object sender, SessionEventArgs e)
    {
        try
        {
            if (_detectionEngine == null) return;

            var result = await _detectionEngine.EvaluateRequestAsync(e);
            if (result != null && result.IsViolation && result.Action == "Block")
            {
                await e.Ok(
                    "<html><body><h1>403 Forbidden</h1><p>SecureGuard DLP: " +
                    System.Net.WebUtility.HtmlEncode(result.Message) +
                    "</p><p>Action telah diblok.</p></body></html>",
                    new Dictionary<string, HttpHeader>
                    {
                        ["Content-Type"] = new HttpHeader("Content-Type", "text/html")
                    },
                    403
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request");
        }
    }

    private Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
    {
        e.IsValid = true;
        return Task.CompletedTask;
    }
}
