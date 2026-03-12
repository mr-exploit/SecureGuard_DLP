using System.ComponentModel.DataAnnotations;

namespace SecureGuard.Server.Models;

public class WhitelistEntry
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string IpAddress { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string CreatedBy { get; set; } = string.Empty;
}
