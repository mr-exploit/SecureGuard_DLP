namespace SecureGuard.Agent.Detection.Models;

public class DetectionResult
{
    public bool IsViolation { get; set; }
    public string ViolationType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string DestinationIp { get; set; } = string.Empty;

    public static DetectionResult NoViolation() => new() { IsViolation = false };

    public static DetectionResult Violation(string violationType, string severity, string action, string message, string details = "")
        => new()
        {
            IsViolation = true,
            ViolationType = violationType,
            Severity = severity,
            Action = action,
            Message = message,
            Details = details
        };
}
