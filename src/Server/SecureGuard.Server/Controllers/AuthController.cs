using Microsoft.AspNetCore.Mvc;
using SecureGuard.Server.Services;

namespace SecureGuard.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { Message = "Email and password are required" });

        var result = await _authService.LoginAsync(request.Email, request.Password);
        if (result == null)
            return Unauthorized(new { Message = "Invalid credentials" });

        return Ok(new
        {
            Token = result.Value.Token,
            User = new
            {
                result.Value.User.Id,
                result.Value.User.Email,
                result.Value.User.FullName,
                result.Value.User.Role
            }
        });
    }
}

public record LoginRequest(string Email, string Password);
