using System.ComponentModel.DataAnnotations;

namespace NiWaterWatch.Api.Contracts;

/// <summary>What a new user submits to create an account.</summary>
/// <param name="Email">The email address to register with. Must be unique.</param>
/// <param name="Password">The plaintext password, as typed by the user — hashed before storage, never stored as-is.</param>
public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password
);