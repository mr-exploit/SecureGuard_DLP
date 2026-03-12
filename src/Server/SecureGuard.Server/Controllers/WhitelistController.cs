using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureGuard.Server.Services;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WhitelistController : ControllerBase
{
    private readonly IWhitelistService _whitelistService;

    public WhitelistController(IWhitelistService whitelistService)
    {
        _whitelistService = whitelistService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWhitelist()
    {
        var entries = await _whitelistService.GetAllAsync();
        return Ok(entries);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEntry(Guid id)
    {
        var entry = await _whitelistService.GetByIdAsync(id);
        if (entry == null) return NotFound();
        return Ok(entry);
    }

    [HttpPost]
    public async Task<IActionResult> AddToWhitelist([FromBody] WhitelistDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entry = await _whitelistService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetEntry), new { id = entry.Id }, entry);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromWhitelist(Guid id)
    {
        var result = await _whitelistService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
