namespace SecureGuard.Shared.DTOs;

public class AgentDto
{
    public string Id { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = "offline";
    public string LastSeen { get; set; } = string.Empty;
    public string Os { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}
