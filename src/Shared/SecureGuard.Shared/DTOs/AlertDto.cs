using System.ComponentModel.DataAnnotations;

namespace SecureGuard.Shared.DTOs;

public class AlertDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    public string AgentId { get; set; } = string.Empty;

    [Required]
    public string Hostname { get; set; } = string.Empty;

    [Required]
    public string ViolationType { get; set; } = string.Empty;

    [Required]
    public string Severity { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }
}
