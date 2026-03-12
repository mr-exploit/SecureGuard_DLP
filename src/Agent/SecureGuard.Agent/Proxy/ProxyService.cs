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
    private readonly ILogger<ProxyService> _logger;
    private readonly AgentOptions _options;
    private readonly SslCertManager _certManager;
    private readonly DetectionEngine _detectionEngine;
    private readonly AlertService _alertService;
    private readonly RemoteLogService _logService;
    private ProxyServer? _proxyServer;

    public ProxyService(
        ILogger<ProxyService> logger,
        IOptions<AgentOptions> options,
        SslCertManager certManager,
        DetectionEngine detectionEngine,
        AlertService alertService,
        RemoteLogService logService)
    {
        _logger = logger;
        _options = options.Value;
        _certManager = certManager;
        _detectionEngine = detectionEngine;
        _alertService = alertService;
        _logService = logService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _proxyServer = new ProxyServer();

        // Set our root CA on the proxy's certificate manager
        _proxyServer.CertificateManager.RootCertificate = _certManager.GetOrCreateRootCa();

        // Wire up validation callbacks on the proxy server
        _proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
        _proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;

        _proxyServer.BeforeRequest += OnBeforeRequest;
        _proxyServer.BeforeResponse += OnBeforeResponse;

        var endpoint = new ExplicitProxyEndPoint(
            System.Net.IPAddress.Parse(_options.ProxyHost),
            _options.ProxyPort,
            decryptSsl: true);

        endpoint.BeforeTunnelConnectRequest += OnBeforeTunnelConnectRequest;

        _proxyServer.AddEndPoint(endpoint);
        _proxyServer.Start();

        _logger.LogInformation("Proxy started on {Host}:{Port}", _options.ProxyHost, _options.ProxyPort);
        return Task.CompletedTask;
    }

    public void Stop()
    {
        if (_proxyServer != null)
        {
            _proxyServer.BeforeRequest -= OnBeforeRequest;
            _proxyServer.BeforeResponse -= OnBeforeResponse;
            _proxyServer.ServerCertificateValidationCallback -= OnCertificateValidation;
            _proxyServer.ClientCertificateSelectionCallback -= OnCertificateSelection;
            _proxyServer.Stop();
            _proxyServer.Dispose();
            _logger.LogInformation("Proxy stopped.");
        }
    }

    private Task OnBeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
    {
        return Task.CompletedTask;
    }

    private async Task OnBeforeRequest(object sender, SessionEventArgs e)
    {
        var request = e.HttpClient.Request;
        var url = request.Url;
        var method = request.Method;
        var contentType = request.ContentType ?? string.Empty;
        var destinationHost = e.HttpClient.Request.RequestUri?.Host ?? string.Empty;

        _logger.LogDebug("Proxy request: {Method} {Url}", method, url);

        if ((method == "POST" || method == "PUT") && contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Blocked image upload to {Url}", url);
            e.Ok("Upload gambar tidak diperbolehkan.");

            var result = new Detection.Models.DetectionResult
            {
                IsBlocked = true,
                ViolationType = Shared.Constants.ViolationTypes.ImageUpload,
                Severity = Shared.Constants.SeverityLevels.High,
                Message = $"Image upload blocked: {url}",
                ProcessName = "browser",
                DestinationIp = destinationHost
            };

            await _alertService.ShowAlertAsync("Upload image tidak diperbolehkan.", result);
            await _logService.SendLogAsync(result);
            return;
        }

        var ipResult = await _detectionEngine.CheckUnknownIpAsync(destinationHost);
        if (ipResult.IsBlocked || ipResult.IsFlagged)
        {
            _logger.LogWarning("Flagged unknown IP: {Host}", destinationHost);
            await _alertService.ShowAlertAsync($"Koneksi ke IP tidak dikenal: {destinationHost}", ipResult);
            await _logService.SendLogAsync(ipResult);
        }
    }

    private async Task OnBeforeResponse(object sender, SessionEventArgs e)
    {
        await Task.CompletedTask;
    }

    private Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
    {
        e.IsValid = true;
        return Task.CompletedTask;
    }

    private Task OnCertificateSelection(object sender, CertificateSelectionEventArgs e)
    {
        return Task.CompletedTask;
    }
}
