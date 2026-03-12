using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureGuard.Server.Data;

namespace SecureGuard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        var totalAgents = await _db.Agents.CountAsync();
        var onlineAgents = await _db.Agents.CountAsync(a => a.IsOnline);
        var incidentsToday = await _db.Alerts.CountAsync(a => a.Timestamp >= today);
        var activeAlerts = await _db.Alerts.CountAsync(a => !a.IsResolved);
        var totalLogs = await _db.LogEntries.CountAsync();
        var logsToday = await _db.LogEntries.CountAsync(l => l.Timestamp >= today);

        var violationBreakdown = await _db.LogEntries
            .Where(l => l.Timestamp >= today.AddDays(-7))
            .GroupBy(l => l.ViolationType)
            .Select(g => new { ViolationType = g.Key, Count = g.Count() })
            .ToListAsync();

        var severityBreakdown = await _db.Alerts
            .Where(a => !a.IsResolved)
            .GroupBy(a => a.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToListAsync();

        var recentAlerts = await _db.Alerts
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .ToListAsync();

        var dailyStats = await _db.LogEntries
            .Where(l => l.Timestamp >= today.AddDays(-7))
            .GroupBy(l => l.Timestamp.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return Ok(new
        {
            TotalAgents = totalAgents,
            OnlineAgents = onlineAgents,
            OfflineAgents = totalAgents - onlineAgents,
            IncidentsToday = incidentsToday,
            ActiveAlerts = activeAlerts,
            TotalLogs = totalLogs,
            LogsToday = logsToday,
            ViolationBreakdown = violationBreakdown,
            SeverityBreakdown = severityBreakdown,
            RecentAlerts = recentAlerts,
            DailyStats = dailyStats
        });
    }
}
