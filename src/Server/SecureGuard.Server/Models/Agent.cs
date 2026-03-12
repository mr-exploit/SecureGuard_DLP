namespace SecureGuard.Server.Models;

public class Agent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Hostname { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = "offline";
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public string Os { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public ICollection<LogEntry> Logs { get; set; } = new List<LogEntry>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
