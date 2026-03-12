namespace SecureGuard.Agent.Detection.Models;

public class DetectionResult
{
    public bool IsBlocked { get; set; }
    public bool IsFlagged { get; set; }
    public string ViolationType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string DestinationIp { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string MatchedPattern { get; set; } = string.Empty;
}
