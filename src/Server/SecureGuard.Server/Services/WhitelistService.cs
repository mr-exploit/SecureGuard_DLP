using Microsoft.EntityFrameworkCore;
using SecureGuard.Server.Data;
using SecureGuard.Server.Models;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Services;

public interface IWhitelistService
{
    Task<IEnumerable<WhitelistEntry>> GetAllAsync();
    Task<WhitelistEntry?> GetByIdAsync(Guid id);
    Task<WhitelistEntry> CreateAsync(WhitelistDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> IsAllowedAsync(string ipAddress);
}

public class WhitelistService : IWhitelistService
{
    private readonly AppDbContext _db;

    public WhitelistService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<WhitelistEntry>> GetAllAsync() =>
        await _db.WhitelistEntries.OrderBy(w => w.IpAddress).ToListAsync();

    public async Task<WhitelistEntry?> GetByIdAsync(Guid id) =>
        await _db.WhitelistEntries.FindAsync(id);

    public async Task<WhitelistEntry> CreateAsync(WhitelistDto dto)
    {
        var existing = await _db.WhitelistEntries
            .FirstOrDefaultAsync(w => w.IpAddress == dto.IpAddress);
        if (existing != null) return existing;

        var entry = new WhitelistEntry
        {
            IpAddress = dto.IpAddress,
            Description = dto.Description,
            CreatedBy = dto.CreatedBy
        };
        _db.WhitelistEntries.Add(entry);
        await _db.SaveChangesAsync();
        return entry;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entry = await _db.WhitelistEntries.FindAsync(id);
        if (entry == null) return false;
        _db.WhitelistEntries.Remove(entry);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsAllowedAsync(string ipAddress)
    {
        return await _db.WhitelistEntries.AnyAsync(w => w.IpAddress == ipAddress);
    }
}
