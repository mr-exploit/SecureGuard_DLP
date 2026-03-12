using Microsoft.Extensions.Options;
using SecureGuard.Agent.Alert;
using SecureGuard.Agent.Detection.Models;
using SecureGuard.Agent.Detection.Rules;
using SecureGuard.Agent.Logging;
using SecureGuard.Shared.Constants;
using Titanium.Web.Proxy.EventArguments;

namespace SecureGuard.Agent.Detection;

public class DetectionEngine
{
    private readonly List<IDetectionRule> _rules;
    private readonly AlertService _alertService;
    private readonly RemoteLogService _logService;
    private readonly ILogger<DetectionEngine> _logger;
    private readonly AgentOptions _options;

    public DetectionEngine(
        AlertService alertService,
        RemoteLogService logService,
        ILogger<DetectionEngine> logger,
        IOptions<AgentOptions> options)
    {
        _alertService = alertService;
        _logService = logService;
        _logger = logger;
        _options = options.Value;

        _rules = new List<IDetectionRule>
        {
            new ImageUploadRule(),
            new CredentialFileRule(),
            new CredentialPatternRule(),
            new UnknownIpRule(_options.WhitelistedIps)
        };
    }

    public async Task<DetectionResult?> EvaluateRequestAsync(SessionEventArgs session)
    {
        foreach (var rule in _rules)
        {
            try
            {
                var result = await rule.EvaluateAsync(session);
                if (result.IsViolation)
                {
                    _logger.LogWarning("Violation detected by {Rule}: {ViolationType} - {Message}",
                        rule.RuleName, result.ViolationType, result.Message);

                    result.ProcessName = GetProcessName(session);

                    await HandleViolationAsync(result);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating rule {Rule}", rule.RuleName);
            }
        }
        return null;
    }

    private async Task HandleViolationAsync(DetectionResult result)
    {
        try
        {
            await _alertService.ShowAlertAsync(result);
            await _logService.SendLogAsync(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling violation");
        }
    }

    private static string GetProcessName(SessionEventArgs session)
    {
        try
        {
            return session.HttpClient.ProcessId.HasValue
                ? System.Diagnostics.Process.GetProcessById(session.HttpClient.ProcessId.Value).ProcessName
                : "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}
