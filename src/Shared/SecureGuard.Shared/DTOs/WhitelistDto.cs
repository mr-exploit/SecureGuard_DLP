namespace SecureGuard.Shared.DTOs;

public class WhitelistDto
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AddedAt { get; set; } = DateTime.UtcNow.ToString("o");
    public bool IsActive { get; set; } = true;
}
