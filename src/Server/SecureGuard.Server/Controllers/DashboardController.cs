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
        var now = DateTime.UtcNow;
        var todayStart = now.Date;

        var totalAgents = await _db.Agents.CountAsync();
        var onlineAgents = await _db.Agents.CountAsync(a => a.LastSeen > now.AddMinutes(-5));
        var incidentsToday = await _db.Logs.CountAsync(l => l.Timestamp >= todayStart);
        var activeAlerts = await _db.Alerts.CountAsync(a => !a.Acknowledged);
        var criticalAlerts = await _db.Alerts.CountAsync(a => !a.Acknowledged && a.Severity == "CRITICAL");

        var violationBreakdown = await _db.Logs
            .Where(l => l.Timestamp >= todayStart)
            .GroupBy(l => l.ViolationType)
            .Select(g => new { violationType = g.Key, count = g.Count() })
            .ToListAsync();

        var recentAlerts = await _db.Alerts
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .Select(a => new
            {
                a.Id,
                timestamp = a.Timestamp.ToString("o"),
                a.Hostname,
                a.ViolationType,
                a.Severity,
                a.Message,
                a.Acknowledged
            })
            .ToListAsync();

        var incidentTrend = await _db.Logs
            .Where(l => l.Timestamp >= now.AddDays(-7))
            .GroupBy(l => l.Timestamp.Date)
            .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), count = g.Count() })
            .OrderBy(x => x.date)
            .ToListAsync();

        return Ok(new
        {
            totalAgents,
            onlineAgents,
            incidentsToday,
            activeAlerts,
            criticalAlerts,
            violationBreakdown,
            recentAlerts,
            incidentTrend
        });
    }
}
