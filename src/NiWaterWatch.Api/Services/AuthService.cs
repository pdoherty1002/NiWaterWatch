using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NiWaterWatch.Api.Contracts;
using NiWaterWatch.Domain.Entities;
using NiWaterWatch.Domain.Interfaces;

namespace NiWaterWatch.Api.Services;

/// <summary>
/// Handles user registration and login — password hashing, verification,
/// and JWT issuing.
/// </summary>
public class AuthService
{
    private readonly IRepository<ApplicationUser, Guid> _userRepo;
    private readonly IConfiguration _configuration;

    /// <summary>Creates the service, given a user repository and app configuration (supplied by dependency injection).</summary>
    public AuthService(IRepository<ApplicationUser, Guid> userRepo, IConfiguration configuration)
    {
        _userRepo = userRepo;
        _configuration = configuration;
    }

    /// <summary>Registers a new user. Returns null if the email is already taken.</summary>
    public async Task<AuthResponse?> RegisterAsync(RegisterRequest requestingUser)
    {
        var existing = await _userRepo.GetByConditionAsync(existingUsers => existingUsers.Email == requestingUser.Email);
        if (existing.Any())
            return null;

        var user = new ApplicationUser
        {
            Email = requestingUser.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(requestingUser.Password)
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        return new AuthResponse(GenerateToken(user), user.Email);
    }

    /// <summary>Logs a user in. Returns null if the email doesn't exist or the password doesn't match.</summary>
    public async Task<AuthResponse?> LoginAsync(LoginRequest requestingUser)
    {
        var matches = await _userRepo.GetByConditionAsync(existingUsers => existingUsers.Email == requestingUser.Email);
        var user = matches.FirstOrDefault();

        if (user is null || !BCrypt.Net.BCrypt.Verify(requestingUser.Password, user.PasswordHash))
            return null;

        return new AuthResponse(GenerateToken(user), user.Email);
    }

    // Builds and signs a JWT for the given user — the piece that turns a
    // successful login into something the client can actually use afterward.
    private string GenerateToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}