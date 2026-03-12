using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureGuard.Server.Services;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PoliciesController : ControllerBase
{
    private readonly IPolicyService _policyService;

    public PoliciesController(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPolicies()
    {
        var policies = await _policyService.GetAllAsync();
        return Ok(policies);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPolicy(Guid id)
    {
        var policy = await _policyService.GetByIdAsync(id);
        if (policy == null) return NotFound();
        return Ok(policy);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePolicy([FromBody] PolicyDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var policy = await _policyService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetPolicy), new { id = policy.Id }, policy);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] PolicyDto dto)
    {
        var policy = await _policyService.UpdateAsync(id, dto);
        if (policy == null) return NotFound();
        return Ok(policy);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePolicy(Guid id)
    {
        var result = await _policyService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
