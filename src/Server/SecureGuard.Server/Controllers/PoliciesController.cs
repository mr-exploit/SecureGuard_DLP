using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureGuard.Server.Data;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PoliciesController : ControllerBase
{
    private readonly AppDbContext _db;

    public PoliciesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetPolicies()
    {
        var policies = await _db.Policies
            .Select(p => new PolicyDto
            {
                Id = p.Id,
                Name = p.Name,
                RuleType = p.RuleType,
                Pattern = p.Pattern,
                Action = p.Action,
                IsEnabled = p.IsEnabled,
                Severity = p.Severity,
                CreatedAt = p.CreatedAt.ToString("o")
            })
            .ToListAsync();
        return Ok(policies);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePolicy([FromBody] PolicyDto dto)
    {
        var policy = new Models.Policy
        {
            Name = dto.Name,
            RuleType = dto.RuleType,
            Pattern = dto.Pattern,
            Action = dto.Action,
            IsEnabled = dto.IsEnabled,
            Severity = dto.Severity,
            CreatedAt = DateTime.UtcNow
        };
        _db.Policies.Add(policy);
        await _db.SaveChangesAsync();
        return Ok(new { id = policy.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePolicy(int id, [FromBody] PolicyDto dto)
    {
        var policy = await _db.Policies.FindAsync(id);
        if (policy == null) return NotFound();

        policy.Name = dto.Name;
        policy.RuleType = dto.RuleType;
        policy.Pattern = dto.Pattern;
        policy.Action = dto.Action;
        policy.IsEnabled = dto.IsEnabled;
        policy.Severity = dto.Severity;
        policy.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePolicy(int id)
    {
        var policy = await _db.Policies.FindAsync(id);
        if (policy == null) return NotFound();
        _db.Policies.Remove(policy);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
