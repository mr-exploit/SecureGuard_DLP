namespace SecureGuard.Shared.DTOs;

public class WhitelistDto
{
    public Guid Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
