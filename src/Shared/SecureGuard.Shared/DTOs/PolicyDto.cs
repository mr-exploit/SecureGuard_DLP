namespace SecureGuard.Shared.DTOs;

public class PolicyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string Action { get; set; } = "block";
    public bool IsEnabled { get; set; } = true;
    public string Severity { get; set; } = "HIGH";
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
}
