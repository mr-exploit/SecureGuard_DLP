namespace SecureGuard.Server.Models;

public class Alert
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string AgentId { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string ViolationType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public bool Acknowledged { get; set; } = false;
    public DateTime? AcknowledgedAt { get; set; }
    public Agent? Agent { get; set; }
}
