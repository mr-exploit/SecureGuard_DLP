using Microsoft.EntityFrameworkCore;
using SecureGuard.Server.Data;
using SecureGuard.Server.Models;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Services;

public interface ILogService
{
    Task<LogEntry> CreateAsync(LogDto dto);
    Task<(IEnumerable<LogEntry> Items, int Total)> GetAllAsync(int page, int pageSize, string? agentId, string? violationType, string? severity, DateTime? from, DateTime? to);
    Task<LogEntry?> GetByIdAsync(Guid id);
}

public class LogService : ILogService
{
    private readonly AppDbContext _db;

    public LogService(AppDbContext db) => _db = db;

    public async Task<LogEntry> CreateAsync(LogDto dto)
    {
        var entry = new LogEntry
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Timestamp = dto.Timestamp,
            AgentId = dto.AgentId,
            Hostname = dto.Hostname,
            Username = dto.Username,
            ProcessName = dto.ProcessName,
            DestinationIp = dto.DestinationIp,
            ViolationType = dto.ViolationType,
            Severity = dto.Severity,
            Details = dto.Details,
            FilePath = dto.FilePath,
            Url = dto.Url
        };

        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.AgentId == dto.AgentId);
        if (agent != null)
        {
            entry.AgentEntityId = agent.Id;
            agent.LastSeen = DateTime.UtcNow;
            agent.IsOnline = true;
        }

        _db.LogEntries.Add(entry);
        await _db.SaveChangesAsync();
        return entry;
    }

    public async Task<(IEnumerable<LogEntry> Items, int Total)> GetAllAsync(
        int page, int pageSize, string? agentId, string? violationType, string? severity,
        DateTime? from, DateTime? to)
    {
        var query = _db.LogEntries.AsQueryable();

        if (!string.IsNullOrEmpty(agentId))
            query = query.Where(l => l.AgentId == agentId);
        if (!string.IsNullOrEmpty(violationType))
            query = query.Where(l => l.ViolationType == violationType);
        if (!string.IsNullOrEmpty(severity))
            query = query.Where(l => l.Severity == severity);
        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<LogEntry?> GetByIdAsync(Guid id) =>
        await _db.LogEntries.FindAsync(id);
}
