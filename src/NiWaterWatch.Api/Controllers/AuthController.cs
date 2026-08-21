using Microsoft.AspNetCore.Mvc;
using NiWaterWatch.Api.Contracts;
using NiWaterWatch.Api.Services;

namespace NiWaterWatch.Api.Controllers;

/// <summary>Registration and login endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    /// <summary>Creates the controller, given its service (supplied by dependency injection).</summary>
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Registers a new user account.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result is null)
            return Conflict("An account with this email already exists.");

        return Ok(result);
    }

    /// <summary>Logs an existing user in.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result is null)
            return Unauthorized("Invalid email or password.");

        return Ok(result);
    }
}