namespace SecureGuard.Shared.DTOs;

public class LogDto
{
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
    public string AgentId { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string DestinationIp { get; set; } = string.Empty;
    public string ViolationType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}
