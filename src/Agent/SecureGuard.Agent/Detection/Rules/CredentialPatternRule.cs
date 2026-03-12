using System.Text.RegularExpressions;
using SecureGuard.Agent.Detection.Models;
using SecureGuard.Shared.Constants;
using Titanium.Web.Proxy.EventArguments;

namespace SecureGuard.Agent.Detection.Rules;

public class CredentialPatternRule : IDetectionRule
{
    public string RuleName => "CredentialPatternRule";

    private static readonly Regex[] CredentialPatterns =
    {
        new(@"AWS_SECRET_ACCESS_KEY\s*[=:]\s*[A-Za-z0-9+/]{40}", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"AKIA[0-9A-Z]{16}", RegexOptions.Compiled),
        new(@"API_KEY\s*[=:]\s*[A-Za-z0-9\-_]{16,}", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"DATABASE_PASSWORD\s*[=:]\s*\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"JWT_SECRET\s*[=:]\s*\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"password\s*[=:]\s*[^\s&]{6,}", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"secret\s*[=:]\s*[^\s&]{6,}", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"token\s*[=:]\s*[A-Za-z0-9\-_\.]{20,}", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"eyJ[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+", RegexOptions.Compiled),  // JWT token format
        new(@"-----BEGIN (RSA |EC )?PRIVATE KEY-----", RegexOptions.Compiled),
        new(@"ghp_[A-Za-z0-9]{36}", RegexOptions.Compiled),  // GitHub personal access token
        new(@"glpat-[A-Za-z0-9\-_]{20}", RegexOptions.Compiled)  // GitLab personal access token
    };

    public async Task<DetectionResult> EvaluateAsync(SessionEventArgs session)
    {
        var request = session.HttpClient.Request;

        if (request.Method != "POST" && request.Method != "PUT")
            return DetectionResult.NoViolation();

        if (!request.HasBody)
            return DetectionResult.NoViolation();

        try
        {
            var body = request.BodyString ?? string.Empty;
            if (string.IsNullOrEmpty(body))
                return DetectionResult.NoViolation();

            // Limit scanning to avoid performance issues with large bodies
            var scanBody = body.Length > 102400 ? body[..102400] : body;

            foreach (var pattern in CredentialPatterns)
            {
                var match = pattern.Match(scanBody);
                if (match.Success)
                {
                    var result = DetectionResult.Violation(
                        ViolationTypes.CredentialPattern,
                        SeverityLevels.Critical,
                        "Block",
                        "Upload data credential terdeteksi dan diblok.",
                        $"Pattern matched: {pattern} at position {match.Index} in request to {request.RequestUri}"
                    );
                    result.Url = request.RequestUri.ToString();
                    return result;
                }
            }
        }
        catch (Exception)
        {
            // If we can't read the body, skip this check
        }

        return DetectionResult.NoViolation();
    }
}
