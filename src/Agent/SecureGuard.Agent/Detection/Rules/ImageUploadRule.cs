using SecureGuard.Agent.Detection.Models;
using SecureGuard.Shared.Constants;
using Titanium.Web.Proxy.EventArguments;

namespace SecureGuard.Agent.Detection.Rules;

public class ImageUploadRule : IDetectionRule
{
    public string RuleName => "ImageUploadRule";

    public Task<DetectionResult> EvaluateAsync(SessionEventArgs session)
    {
        var request = session.HttpClient.Request;

        // Only check POST/PUT requests (uploads)
        if (request.Method != "POST" && request.Method != "PUT")
            return Task.FromResult(DetectionResult.NoViolation());

        var contentType = request.ContentType ?? string.Empty;
        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            var result = DetectionResult.Violation(
                ViolationTypes.ImageUpload,
                SeverityLevels.High,
                "Block",
                "Upload image tidak diperbolehkan.",
                $"Attempted to upload image with content-type: {contentType} to {request.RequestUri}"
            );
            result.Url = request.RequestUri.ToString();
            return Task.FromResult(result);
        }

        // Check multipart form data for image files
        if (contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            // Check by filename in content-disposition headers
            if (request.HasBody)
            {
                var body = request.BodyString ?? string.Empty;
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".ico", ".tiff" };
                foreach (var ext in imageExtensions)
                {
                    if (body.Contains($"filename=\"", StringComparison.OrdinalIgnoreCase) &&
                        body.Contains(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        var result2 = DetectionResult.Violation(
                            ViolationTypes.ImageUpload,
                            SeverityLevels.High,
                            "Block",
                            "Upload image tidak diperbolehkan.",
                            $"Attempted to upload image file ({ext}) to {request.RequestUri}"
                        );
                        result2.Url = request.RequestUri.ToString();
                        return Task.FromResult(result2);
                    }
                }
            }
        }

        return Task.FromResult(DetectionResult.NoViolation());
    }
}
