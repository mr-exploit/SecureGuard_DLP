using SecureGuard.Agent.Detection.Models;
using SecureGuard.Shared.Constants;
using System.Text.RegularExpressions;

namespace SecureGuard.Agent.Detection.Rules;

public class CredentialPatternRule : IDetectionRule
{
    public string RuleName => "CredentialPattern";

    private static readonly (Regex Pattern, string Name)[] CredentialPatterns =
    {
        (new Regex(@"AWS_SECRET_ACCESS_KEY\s*[:=]\s*\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled), "AWS Secret Key"),
        (new Regex(@"AWS_ACCESS_KEY_ID\s*[:=]\s*[A-Z0-9]{20}", RegexOptions.IgnoreCase | RegexOptions.Compiled), "AWS Access Key ID"),
        (new Regex(@"API_KEY\s*[:=]\s*\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled), "API Key"),
        (new Regex(@"DATABASE_PASSWORD\s*[:=]\s*\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled), "Database Password"),
        (new Regex(@"JWT_SECRET\s*[:=]\s*\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled), "JWT Secret"),
        (new Regex(@"password\s*[=:]\s*\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled), "Password"),
        (new Regex(@"secret\s*[=:]\s*\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled), "Secret"),
        (new Regex(@"token\s*[=:]\s*\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled), "Token"),
        (new Regex(@"AKIA[0-9A-Z]{16}", RegexOptions.Compiled), "AWS Access Key"),
        (new Regex(@"eyJ[A-Za-z0-9\-_]+\.eyJ[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+", RegexOptions.Compiled), "JWT Token"),
        (new Regex(@"-----BEGIN [A-Z ]+-----", RegexOptions.Compiled), "PEM Key"),
    };

    public Task<DetectionResult> EvaluateAsync(DetectionContext context)
    {
        var content = context.FileContent ?? string.Empty;
        if (string.IsNullOrEmpty(content))
            return Task.FromResult(new DetectionResult { IsBlocked = false });

        foreach (var (pattern, name) in CredentialPatterns)
        {
            if (pattern.IsMatch(content))
            {
                return Task.FromResult(new DetectionResult
                {
                    IsBlocked = true,
                    ViolationType = ViolationTypes.CredentialPattern,
                    Severity = SeverityLevels.Critical,
                    Message = $"Credential pattern detected: {name}",
                    ProcessName = context.ProcessName ?? "unknown",
                    FilePath = context.FileName ?? string.Empty,
                    MatchedPattern = name
                });
            }
        }

        return Task.FromResult(new DetectionResult { IsBlocked = false });
    }
}
