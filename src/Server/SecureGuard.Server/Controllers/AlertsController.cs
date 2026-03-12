using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SecureGuard.Server.Data;
using SecureGuard.Server.Hubs;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<AlertHub> _hubContext;

    public AlertsController(AppDbContext db, IHubContext<AlertHub> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAlert([FromBody] AlertDto dto)
    {
        var alert = new Models.Alert
        {
            Timestamp = DateTime.TryParse(dto.Timestamp, out var ts) ? ts : DateTime.UtcNow,
            AgentId = dto.AgentId,
            Hostname = dto.Hostname,
            ViolationType = dto.ViolationType,
            Severity = dto.Severity,
            Message = dto.Message,
            Details = dto.Details,
            Acknowledged = false
        };

        _db.Alerts.Add(alert);
        await _db.SaveChangesAsync();

        // Broadcast alert via SignalR
        await _hubContext.Clients.All.SendAsync("NewAlert", dto);

        return Ok(new { id = alert.Id });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? acknowledged = null,
        [FromQuery] string? severity = null)
    {
        var query = _db.Alerts.AsQueryable();
        if (acknowledged.HasValue) query = query.Where(a => a.Acknowledged == acknowledged.Value);
        if (!string.IsNullOrEmpty(severity)) query = query.Where(a => a.Severity == severity);

        var total = await query.CountAsync();
        var alerts = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AlertDto
            {
                Timestamp = a.Timestamp.ToString("o"),
                AgentId = a.AgentId,
                Hostname = a.Hostname,
                ViolationType = a.ViolationType,
                Severity = a.Severity,
                Message = a.Message,
                Details = a.Details,
                Acknowledged = a.Acknowledged
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, data = alerts });
    }

    [HttpPut("{id}/acknowledge")]
    [Authorize]
    public async Task<IActionResult> AcknowledgeAlert(int id)
    {
        var alert = await _db.Alerts.FindAsync(id);
        if (alert == null) return NotFound();
        alert.Acknowledged = true;
        alert.AcknowledgedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok();
    }
}
