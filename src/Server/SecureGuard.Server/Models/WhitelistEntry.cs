namespace SecureGuard.Server.Models;

public class WhitelistEntry
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
