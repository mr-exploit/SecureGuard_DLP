using Microsoft.EntityFrameworkCore;
using SecureGuard.Server.Data;
using SecureGuard.Server.Models;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Services;

public interface IAlertService
{
    Task<Alert> CreateAsync(AlertDto dto);
    Task<(IEnumerable<Alert> Items, int Total)> GetAllAsync(int page, int pageSize, bool? isResolved, string? severity);
    Task<Alert?> GetByIdAsync(Guid id);
    Task<Alert?> ResolveAsync(Guid id);
}

public class AlertService : IAlertService
{
    private readonly AppDbContext _db;

    public AlertService(AppDbContext db) => _db = db;

    public async Task<Alert> CreateAsync(AlertDto dto)
    {
        var alert = new Alert
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Timestamp = dto.Timestamp,
            AgentId = dto.AgentId,
            Hostname = dto.Hostname,
            ViolationType = dto.ViolationType,
            Severity = dto.Severity,
            Message = dto.Message,
            Details = dto.Details,
            IsResolved = dto.IsResolved
        };

        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.AgentId == dto.AgentId);
        if (agent != null)
        {
            alert.AgentEntityId = agent.Id;
            agent.LastSeen = DateTime.UtcNow;
            agent.IsOnline = true;
        }

        _db.Alerts.Add(alert);
        await _db.SaveChangesAsync();
        return alert;
    }

    public async Task<(IEnumerable<Alert> Items, int Total)> GetAllAsync(
        int page, int pageSize, bool? isResolved, string? severity)
    {
        var query = _db.Alerts.AsQueryable();

        if (isResolved.HasValue)
            query = query.Where(a => a.IsResolved == isResolved.Value);
        if (!string.IsNullOrEmpty(severity))
            query = query.Where(a => a.Severity == severity);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Alert?> GetByIdAsync(Guid id) =>
        await _db.Alerts.FindAsync(id);

    public async Task<Alert?> ResolveAsync(Guid id)
    {
        var alert = await _db.Alerts.FindAsync(id);
        if (alert == null) return null;

        alert.IsResolved = true;
        alert.ResolvedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return alert;
    }
}
