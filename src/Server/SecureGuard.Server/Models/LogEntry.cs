namespace SecureGuard.Server.Models;

public class LogEntry
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string AgentId { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string DestinationIp { get; set; } = string.Empty;
    public string ViolationType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public Agent? Agent { get; set; }
}
