using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureGuard.Server.Data;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly AppDbContext _db;

    public LogsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLog([FromBody] LogDto dto)
    {
        var log = new Models.LogEntry
        {
            Timestamp = DateTime.TryParse(dto.Timestamp, out var ts) ? ts : DateTime.UtcNow,
            AgentId = dto.AgentId,
            Hostname = dto.Hostname,
            Username = dto.Username,
            ProcessName = dto.ProcessName,
            DestinationIp = dto.DestinationIp,
            ViolationType = dto.ViolationType,
            Severity = dto.Severity,
            Details = dto.Details
        };

        // Upsert agent
        var agent = await _db.Agents.FindAsync(dto.AgentId);
        if (agent == null && !string.IsNullOrEmpty(dto.AgentId))
        {
            agent = new Models.Agent
            {
                Id = dto.AgentId,
                Hostname = dto.Hostname,
                Username = dto.Username,
                Status = "online",
                LastSeen = DateTime.UtcNow
            };
            _db.Agents.Add(agent);
        }
        else if (agent != null)
        {
            agent.LastSeen = DateTime.UtcNow;
            agent.Status = "online";
        }

        _db.Logs.Add(log);
        await _db.SaveChangesAsync();
        return Ok(new { id = log.Id });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? agentId = null,
        [FromQuery] string? violationType = null,
        [FromQuery] string? severity = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var query = _db.Logs.AsQueryable();

        if (!string.IsNullOrEmpty(agentId)) query = query.Where(l => l.AgentId == agentId);
        if (!string.IsNullOrEmpty(violationType)) query = query.Where(l => l.ViolationType == violationType);
        if (!string.IsNullOrEmpty(severity)) query = query.Where(l => l.Severity == severity);
        if (from.HasValue) query = query.Where(l => l.Timestamp >= from.Value);
        if (to.HasValue) query = query.Where(l => l.Timestamp <= to.Value);

        var total = await query.CountAsync();
        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LogDto
            {
                Timestamp = l.Timestamp.ToString("o"),
                AgentId = l.AgentId,
                Hostname = l.Hostname,
                Username = l.Username,
                ProcessName = l.ProcessName,
                DestinationIp = l.DestinationIp,
                ViolationType = l.ViolationType,
                Severity = l.Severity,
                Details = l.Details
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, data = logs });
    }
}
