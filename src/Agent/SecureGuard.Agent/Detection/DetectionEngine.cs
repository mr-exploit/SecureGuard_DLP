using Microsoft.Extensions.Options;
using SecureGuard.Agent.Detection.Models;
using SecureGuard.Agent.Detection.Rules;

namespace SecureGuard.Agent.Detection;

public class DetectionEngine
{
    private readonly ILogger<DetectionEngine> _logger;
    private readonly IEnumerable<IDetectionRule> _rules;
    private readonly UnknownIpRule _unknownIpRule;

    public DetectionEngine(
        ILogger<DetectionEngine> logger,
        IOptions<AgentOptions> options)
    {
        _logger = logger;
        _unknownIpRule = new UnknownIpRule(options);
        _rules = new IDetectionRule[]
        {
            new ImageUploadRule(),
            new CredentialFileRule(),
            _unknownIpRule,
            new CredentialPatternRule()
        };
    }

    public async Task<DetectionResult> EvaluateAsync(DetectionContext context)
    {
        foreach (var rule in _rules)
        {
            var result = await rule.EvaluateAsync(context);
            if (result.IsBlocked || result.IsFlagged)
            {
                _logger.LogWarning("Rule {Rule} triggered: {Message}", rule.RuleName, result.Message);
                return result;
            }
        }

        return new DetectionResult { IsBlocked = false };
    }

    public async Task<DetectionResult> CheckUnknownIpAsync(string host)
    {
        var context = new DetectionContext { DestinationIp = host };
        return await _unknownIpRule.EvaluateAsync(context);
    }

    public async Task<DetectionResult> ScanFileContentAsync(string filePath, string content, string? processName = null)
    {
        var fileRule = new CredentialFileRule();
        var fileCtx = new DetectionContext { FileName = filePath, ProcessName = processName };
        var fileResult = await fileRule.EvaluateAsync(fileCtx);
        if (fileResult.IsBlocked) return fileResult;

        var patternRule = new CredentialPatternRule();
        var patternCtx = new DetectionContext { FileName = filePath, FileContent = content, ProcessName = processName };
        return await patternRule.EvaluateAsync(patternCtx);
    }
}
