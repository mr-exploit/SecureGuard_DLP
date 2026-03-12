using System.ComponentModel.DataAnnotations;

namespace SecureGuard.Shared.DTOs;

public class LogDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    public string AgentId { get; set; } = string.Empty;

    [Required]
    public string Hostname { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty;

    public string ProcessName { get; set; } = string.Empty;
    public string DestinationIp { get; set; } = string.Empty;

    [Required]
    public string ViolationType { get; set; } = string.Empty;

    [Required]
    public string Severity { get; set; } = string.Empty;

    public string Details { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
