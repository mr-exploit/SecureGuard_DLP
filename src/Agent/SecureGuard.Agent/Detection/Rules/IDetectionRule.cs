using SecureGuard.Agent.Detection.Models;

namespace SecureGuard.Agent.Detection.Rules;

public interface IDetectionRule
{
    string RuleName { get; }
    Task<DetectionResult> EvaluateAsync(DetectionContext context);
}

public class DetectionContext
{
    public string? Url { get; set; }
    public string? ContentType { get; set; }
    public string? FileName { get; set; }
    public string? FileContent { get; set; }
    public string? DestinationIp { get; set; }
    public string? ProcessName { get; set; }
    public int? ProcessId { get; set; }
}
