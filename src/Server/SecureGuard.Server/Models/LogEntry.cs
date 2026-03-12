using System.ComponentModel.DataAnnotations;

namespace SecureGuard.Server.Models;

public class LogEntry
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(100)]
    public string AgentId { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Hostname { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(255)]
    public string ProcessName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string DestinationIp { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ViolationType { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Severity { get; set; } = string.Empty;

    public string Details { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    public Guid? AgentEntityId { get; set; }
    public Agent? AgentEntity { get; set; }
}
