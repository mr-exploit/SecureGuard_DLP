using Microsoft.EntityFrameworkCore;
using SecureGuard.Server.Data;
using SecureGuard.Server.Models;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Services;

public interface IPolicyService
{
    Task<IEnumerable<Policy>> GetAllAsync();
    Task<Policy?> GetByIdAsync(Guid id);
    Task<Policy> CreateAsync(PolicyDto dto);
    Task<Policy?> UpdateAsync(Guid id, PolicyDto dto);
    Task<bool> DeleteAsync(Guid id);
}

public class PolicyService : IPolicyService
{
    private readonly AppDbContext _db;

    public PolicyService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Policy>> GetAllAsync() =>
        await _db.Policies.OrderBy(p => p.Name).ToListAsync();

    public async Task<Policy?> GetByIdAsync(Guid id) =>
        await _db.Policies.FindAsync(id);

    public async Task<Policy> CreateAsync(PolicyDto dto)
    {
        var policy = new Policy
        {
            Name = dto.Name,
            Description = dto.Description,
            RuleType = dto.RuleType,
            Action = dto.Action,
            IsEnabled = dto.IsEnabled,
            Parameters = dto.Parameters
        };
        _db.Policies.Add(policy);
        await _db.SaveChangesAsync();
        return policy;
    }

    public async Task<Policy?> UpdateAsync(Guid id, PolicyDto dto)
    {
        var policy = await _db.Policies.FindAsync(id);
        if (policy == null) return null;

        policy.Name = dto.Name;
        policy.Description = dto.Description;
        policy.RuleType = dto.RuleType;
        policy.Action = dto.Action;
        policy.IsEnabled = dto.IsEnabled;
        policy.Parameters = dto.Parameters;
        policy.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return policy;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var policy = await _db.Policies.FindAsync(id);
        if (policy == null) return false;
        _db.Policies.Remove(policy);
        await _db.SaveChangesAsync();
        return true;
    }
}
