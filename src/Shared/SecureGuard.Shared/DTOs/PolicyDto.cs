namespace SecureGuard.Shared.DTOs;

public class PolicyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string Parameters { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
