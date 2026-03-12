namespace SecureGuard.Shared.DTOs;

public class AgentDto
{
    public Guid Id { get; set; }
    public string AgentId { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string AgentVersion { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public DateTime LastSeen { get; set; }
    public DateTime RegisteredAt { get; set; }
    public AgentConfigDto? Config { get; set; }
}

public class AgentConfigDto
{
    public bool ProxyEnabled { get; set; } = true;
    public int ProxyPort { get; set; } = 8877;
    public bool FileMonitorEnabled { get; set; } = true;
    public bool ProcessMonitorEnabled { get; set; } = true;
    public string ServerUrl { get; set; } = string.Empty;
    public List<string> WhitelistedIps { get; set; } = new();
}
