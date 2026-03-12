using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureGuard.Server.Services;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly IAgentService _agentService;

    public AgentsController(IAgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] AgentDto dto)
    {
        var agent = await _agentService.RegisterOrUpdateAsync(dto);
        return Ok(agent);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAgents()
    {
        var agents = await _agentService.GetAllAsync();
        return Ok(agents);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetAgent(Guid id)
    {
        var agent = await _agentService.GetByIdAsync(id);
        if (agent == null) return NotFound();
        return Ok(agent);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateAgent(Guid id, [FromBody] AgentConfigDto config)
    {
        var agent = await _agentService.UpdateConfigAsync(id, config);
        if (agent == null) return NotFound();
        return Ok(agent);
    }
}
