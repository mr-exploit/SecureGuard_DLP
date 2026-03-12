namespace SecureGuard.Shared.DTOs;

public class AlertDto
{
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
    public string AgentId { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string ViolationType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public bool Acknowledged { get; set; } = false;
}
