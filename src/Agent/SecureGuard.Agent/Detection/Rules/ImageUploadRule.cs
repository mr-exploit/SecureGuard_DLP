using SecureGuard.Agent.Detection.Models;
using SecureGuard.Shared.Constants;

namespace SecureGuard.Agent.Detection.Rules;

public class ImageUploadRule : IDetectionRule
{
    public string RuleName => "ImageUpload";

    public Task<DetectionResult> EvaluateAsync(DetectionContext context)
    {
        if (!string.IsNullOrEmpty(context.ContentType) &&
            context.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new DetectionResult
            {
                IsBlocked = true,
                ViolationType = ViolationTypes.ImageUpload,
                Severity = SeverityLevels.High,
                Message = $"Image upload detected: {context.ContentType}",
                ProcessName = context.ProcessName ?? "unknown",
                DestinationIp = context.DestinationIp ?? string.Empty
            });
        }

        return Task.FromResult(new DetectionResult { IsBlocked = false });
    }
}
