using System.ComponentModel.DataAnnotations;

namespace SecureGuard.Server.Models;

public class Agent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string AgentId { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Hostname { get; set; } = string.Empty;

    [MaxLength(50)]
    public string IpAddress { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(255)]
    public string OsVersion { get; set; } = string.Empty;

    [MaxLength(50)]
    public string AgentVersion { get; set; } = string.Empty;

    public bool IsOnline { get; set; } = false;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public bool ProxyEnabled { get; set; } = true;
    public int ProxyPort { get; set; } = 8877;
    public bool FileMonitorEnabled { get; set; } = true;
    public bool ProcessMonitorEnabled { get; set; } = true;

    public ICollection<LogEntry> Logs { get; set; } = new List<LogEntry>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
