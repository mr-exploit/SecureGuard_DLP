using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SecureGuard.Server.Hubs;
using SecureGuard.Server.Services;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly ILogService _logService;
    private readonly IHubContext<AlertHub> _hubContext;

    public LogsController(ILogService logService, IHubContext<AlertHub> hubContext)
    {
        _logService = logService;
        _hubContext = hubContext;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateLog([FromBody] LogDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entry = await _logService.CreateAsync(dto);
        await _hubContext.Clients.All.SendAsync("ReceiveLog", dto);
        return CreatedAtAction(nameof(GetLog), new { id = entry.Id }, entry);
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
        var (items, total) = await _logService.GetAllAsync(page, pageSize, agentId, violationType, severity, from, to);
        return Ok(new
        {
            Data = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        });
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetLog(Guid id)
    {
        var entry = await _logService.GetByIdAsync(id);
        if (entry == null) return NotFound();
        return Ok(entry);
    }
}
