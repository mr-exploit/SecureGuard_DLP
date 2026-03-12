using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SecureGuard.Server.Hubs;
using SecureGuard.Server.Services;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;
    private readonly IHubContext<AlertHub> _hubContext;

    public AlertsController(IAlertService alertService, IHubContext<AlertHub> hubContext)
    {
        _alertService = alertService;
        _hubContext = hubContext;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAlert([FromBody] AlertDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var alert = await _alertService.CreateAsync(dto);
        await _hubContext.Clients.All.SendAsync("ReceiveAlert", dto);
        return CreatedAtAction(nameof(GetAlert), new { id = alert.Id }, alert);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? isResolved = null,
        [FromQuery] string? severity = null)
    {
        var (items, total) = await _alertService.GetAllAsync(page, pageSize, isResolved, severity);
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
    public async Task<IActionResult> GetAlert(Guid id)
    {
        var alert = await _alertService.GetByIdAsync(id);
        if (alert == null) return NotFound();
        return Ok(alert);
    }

    [HttpPost("{id}/resolve")]
    [Authorize]
    public async Task<IActionResult> ResolveAlert(Guid id)
    {
        var alert = await _alertService.ResolveAsync(id);
        if (alert == null) return NotFound();
        await _hubContext.Clients.All.SendAsync("AlertResolved", id);
        return Ok(alert);
    }
}
