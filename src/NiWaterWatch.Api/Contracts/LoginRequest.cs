using System.ComponentModel.DataAnnotations;

namespace NiWaterWatch.Api.Contracts;

/// <summary>What an existing user submits to log in.</summary>
/// <param name="Email">The account's email address.</param>
/// <param name="Password">The plaintext password, as typed by the user — checked against the stored hash, never compared directly.</param>
public record LoginRequest(
    [Required] string Email,
    [Required] string Password
);