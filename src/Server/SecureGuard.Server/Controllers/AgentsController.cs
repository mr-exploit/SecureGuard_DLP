using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureGuard.Server.Data;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AgentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAgents()
    {
        var agents = await _db.Agents
            .Select(a => new AgentDto
            {
                Id = a.Id,
                Hostname = a.Hostname,
                IpAddress = a.IpAddress,
                Version = a.Version,
                Status = a.LastSeen > DateTime.UtcNow.AddMinutes(-5) ? "online" : "offline",
                LastSeen = a.LastSeen.ToString("o"),
                Os = a.Os,
                Username = a.Username
            })
            .ToListAsync();

        return Ok(agents);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAgent(string id)
    {
        var agent = await _db.Agents.FindAsync(id);
        if (agent == null) return NotFound();

        return Ok(new AgentDto
        {
            Id = agent.Id,
            Hostname = agent.Hostname,
            IpAddress = agent.IpAddress,
            Version = agent.Version,
            Status = agent.LastSeen > DateTime.UtcNow.AddMinutes(-5) ? "online" : "offline",
            LastSeen = agent.LastSeen.ToString("o"),
            Os = agent.Os,
            Username = agent.Username
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAgent(string id, [FromBody] AgentDto dto)
    {
        var agent = await _db.Agents.FindAsync(id);
        if (agent == null) return NotFound();

        agent.Hostname = dto.Hostname;
        agent.IpAddress = dto.IpAddress;
        agent.Version = dto.Version;
        agent.Os = dto.Os;
        agent.Username = dto.Username;

        await _db.SaveChangesAsync();
        return Ok();
    }
}
