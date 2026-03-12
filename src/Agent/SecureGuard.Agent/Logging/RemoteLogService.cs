using Microsoft.Extensions.Options;
using SecureGuard.Agent.Detection.Models;
using SecureGuard.Shared.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace SecureGuard.Agent.Logging;

public class RemoteLogService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RemoteLogService> _logger;
    private readonly AgentOptions _options;
    private readonly string _agentId;
    private readonly string _hostname;
    private readonly string _username;

    public RemoteLogService(
        ILogger<RemoteLogService> logger,
        IOptions<AgentOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _agentId = _options.AgentId;
        _hostname = Environment.MachineName;
        _username = Environment.UserName;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_options.ServerUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public async Task SendLogAsync(DetectionResult result)
    {
        try
        {
            var logDto = new LogDto
            {
                Timestamp = DateTime.UtcNow,
                AgentId = _agentId,
                Hostname = _hostname,
                Username = _username,
                ProcessName = result.ProcessName,
                DestinationIp = result.DestinationIp,
                ViolationType = result.ViolationType,
                Severity = result.Severity,
                Details = result.Details,
                FilePath = result.FilePath,
                Url = result.Url
            };

            var response = await _httpClient.PostAsJsonAsync("/api/logs", logDto);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to send log to server: {StatusCode}", response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Cannot reach server to send log");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending log to server");
        }
    }

    public async Task SendAlertAsync(DetectionResult result)
    {
        try
        {
            var alertDto = new AlertDto
            {
                Timestamp = DateTime.UtcNow,
                AgentId = _agentId,
                Hostname = _hostname,
                ViolationType = result.ViolationType,
                Severity = result.Severity,
                Message = result.Message,
                Details = result.Details
            };

            var response = await _httpClient.PostAsJsonAsync("/api/alerts", alertDto);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to send alert to server: {StatusCode}", response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Cannot reach server to send alert");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending alert to server");
        }
    }

    public async Task RegisterAgentAsync()
    {
        try
        {
            var agentDto = new AgentDto
            {
                AgentId = _agentId,
                Hostname = _hostname,
                IpAddress = GetLocalIpAddress(),
                Username = _username,
                OsVersion = Environment.OSVersion.VersionString,
                AgentVersion = "1.0.0"
            };

            var response = await _httpClient.PostAsJsonAsync("/api/agents/register", agentDto);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Agent registered with server successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to register agent with server");
        }
    }

    private static string GetLocalIpAddress()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip.ToString();
            }
        }
        catch { }
        return "127.0.0.1";
    }
}
