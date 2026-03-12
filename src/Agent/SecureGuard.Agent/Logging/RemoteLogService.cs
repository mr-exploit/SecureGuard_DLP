using Microsoft.Extensions.Options;
using SecureGuard.Agent.Detection.Models;
using SecureGuard.Shared.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace SecureGuard.Agent.Logging;

public class RemoteLogService
{
    private readonly ILogger<RemoteLogService> _logger;
    private readonly AgentOptions _options;
    private readonly HttpClient _httpClient;

    public RemoteLogService(ILogger<RemoteLogService> logger, IOptions<AgentOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_options.ServerUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _options.ApiKey);
        }
    }

    public async Task SendLogAsync(DetectionResult result)
    {
        try
        {
            var log = new LogDto
            {
                Timestamp = DateTime.UtcNow.ToString("o"),
                AgentId = _options.AgentId,
                Hostname = Environment.MachineName,
                Username = Environment.UserName,
                ProcessName = result.ProcessName,
                DestinationIp = result.DestinationIp,
                ViolationType = result.ViolationType,
                Severity = result.Severity,
                Details = result.Message
            };

            var response = await _httpClient.PostAsJsonAsync("/api/logs", log);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to send log to server. Status: {Status}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send log to remote server. Will retry later.");
        }
    }

    public async Task SendAlertAsync(DetectionResult result, string message)
    {
        try
        {
            var alert = new AlertDto
            {
                Timestamp = DateTime.UtcNow.ToString("o"),
                AgentId = _options.AgentId,
                Hostname = Environment.MachineName,
                ViolationType = result.ViolationType,
                Severity = result.Severity,
                Message = message,
                Details = result.Message
            };

            var response = await _httpClient.PostAsJsonAsync("/api/alerts", alert);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to send alert to server. Status: {Status}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send alert to remote server.");
        }
    }
}
