using System.ComponentModel.DataAnnotations;

namespace SecureGuard.Server.Models;

public class Alert
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
    [MaxLength(100)]
    public string ViolationType { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Severity { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }

    public Guid? AgentEntityId { get; set; }
    public Agent? AgentEntity { get; set; }
}
