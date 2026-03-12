using SecureGuard.Agent.Detection.Models;
using SecureGuard.Shared.Constants;
using Titanium.Web.Proxy.EventArguments;

namespace SecureGuard.Agent.Detection.Rules;

public class CredentialFileRule : IDetectionRule
{
    public string RuleName => "CredentialFileRule";

    private static readonly string[] SensitiveFileNames =
    {
        ".env",
        "app.config",
        "secrets.json",
        "credentials.json",
        "web.config",
        ".env.local",
        ".env.production",
        ".env.development"
    };

    private static readonly string[] SensitiveExtensions =
    {
        ".pem",
        ".key",
        ".pfx",
        ".p12",
        ".cer",
        ".crt"
    };

    public Task<DetectionResult> EvaluateAsync(SessionEventArgs session)
    {
        var request = session.HttpClient.Request;

        if (request.Method != "POST" && request.Method != "PUT")
            return Task.FromResult(DetectionResult.NoViolation());

        var url = request.RequestUri.ToString().ToLowerInvariant();
        var contentType = request.ContentType ?? string.Empty;

        // Check URL for sensitive filenames
        foreach (var fileName in SensitiveFileNames)
        {
            if (url.Contains(fileName, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(CreateViolation(fileName, request.RequestUri.ToString()));
            }
        }

        // Check extensions in URL
        foreach (var ext in SensitiveExtensions)
        {
            if (url.Contains(ext, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(CreateViolation(ext, request.RequestUri.ToString()));
            }
        }

        // Check multipart uploads
        if (contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase) && request.HasBody)
        {
            var body = request.BodyString ?? string.Empty;
            foreach (var fileName in SensitiveFileNames)
            {
                if (body.Contains(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(CreateViolation(fileName, request.RequestUri.ToString()));
                }
            }
        }

        return Task.FromResult(DetectionResult.NoViolation());
    }

    private static DetectionResult CreateViolation(string fileName, string url)
    {
        var result = DetectionResult.Violation(
            ViolationTypes.CredentialFile,
            SeverityLevels.Critical,
            "Block",
            "Upload file credential tidak diperbolehkan.",
            $"Attempted to upload credential file: {fileName} to {url}"
        );
        result.Url = url;
        return result;
    }
}
