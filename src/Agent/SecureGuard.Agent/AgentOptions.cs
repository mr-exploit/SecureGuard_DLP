namespace SecureGuard.Agent;

public class AgentOptions
{
    public string AgentId { get; set; } = Guid.NewGuid().ToString();
    public string ServerUrl { get; set; } = "https://localhost:5001";
    public string ProxyHost { get; set; } = "127.0.0.1";
    public int ProxyPort { get; set; } = 8877;
    public string[] WatchPaths { get; set; } = { "C:\\Users" };
    public string[] WhitelistedIps { get; set; } = { "127.0.0.1", "::1" };
    public string ApiKey { get; set; } = string.Empty;
}
