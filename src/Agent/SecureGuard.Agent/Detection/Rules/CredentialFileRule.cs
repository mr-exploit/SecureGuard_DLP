using SecureGuard.Agent.Detection.Models;
using SecureGuard.Shared.Constants;

namespace SecureGuard.Agent.Detection.Rules;

public class CredentialFileRule : IDetectionRule
{
    public string RuleName => "CredentialFile";

    private static readonly HashSet<string> SensitiveFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".env",
        "app.config",
        "secrets.json",
        "credentials.json",
        "web.config",
        ".env.local",
        ".env.production",
        "appsettings.json"
    };

    private static readonly HashSet<string> SensitiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".env",
        ".config",
        ".pfx",
        ".p12",
        ".key",
        ".pem"
    };

    public Task<DetectionResult> EvaluateAsync(DetectionContext context)
    {
        if (string.IsNullOrEmpty(context.FileName))
            return Task.FromResult(new DetectionResult { IsBlocked = false });

        var fileName = Path.GetFileName(context.FileName);
        var extension = Path.GetExtension(context.FileName);

        if (SensitiveFileNames.Contains(fileName) || SensitiveExtensions.Contains(extension))
        {
            return Task.FromResult(new DetectionResult
            {
                IsBlocked = true,
                ViolationType = ViolationTypes.CredentialFile,
                Severity = SeverityLevels.Critical,
                Message = $"Credential file access detected: {context.FileName}",
                ProcessName = context.ProcessName ?? "unknown",
                FilePath = context.FileName
            });
        }

        return Task.FromResult(new DetectionResult { IsBlocked = false });
    }
}
