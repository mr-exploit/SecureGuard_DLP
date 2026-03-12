using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureGuard.Server.Data;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WhitelistController : ControllerBase
{
    private readonly AppDbContext _db;

    public WhitelistController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetWhitelist()
    {
        var entries = await _db.WhitelistEntries
            .Select(w => new WhitelistDto
            {
                Id = w.Id,
                IpAddress = w.IpAddress,
                Description = w.Description,
                AddedAt = w.AddedAt.ToString("o"),
                IsActive = w.IsActive
            })
            .ToListAsync();
        return Ok(entries);
    }

    [HttpPost]
    public async Task<IActionResult> AddToWhitelist([FromBody] WhitelistDto dto)
    {
        if (await _db.WhitelistEntries.AnyAsync(w => w.IpAddress == dto.IpAddress))
            return Conflict(new { message = "IP already in whitelist." });

        var entry = new Models.WhitelistEntry
        {
            IpAddress = dto.IpAddress,
            Description = dto.Description,
            AddedAt = DateTime.UtcNow,
            IsActive = dto.IsActive
        };
        _db.WhitelistEntries.Add(entry);
        await _db.SaveChangesAsync();
        return Ok(new { id = entry.Id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromWhitelist(int id)
    {
        var entry = await _db.WhitelistEntries.FindAsync(id);
        if (entry == null) return NotFound();
        _db.WhitelistEntries.Remove(entry);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
