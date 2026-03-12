namespace SecureGuard.Agent;

public class AgentOptions
{
    public string AgentId { get; set; } = "AGENT-001";
    public string ServerUrl { get; set; } = "http://localhost:5000";
    public int ProxyPort { get; set; } = 8877;
    public bool ProxyEnabled { get; set; } = true;
    public bool FileMonitorEnabled { get; set; } = true;
    public bool ProcessMonitorEnabled { get; set; } = true;
    public List<string> MonitoredPaths { get; set; } = new() { "C:\\Users" };
    public List<string> WhitelistedIps { get; set; } = new() { "127.0.0.1", "::1" };
}
