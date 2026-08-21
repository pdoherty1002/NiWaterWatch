namespace NiWaterWatch.Api.Contracts;

/// <summary>Returned after a successful register or login — the JWT the client uses for authenticated requests from here on.</summary>
/// <param name="Token">The signed JWT. Sent back on later requests to prove who the caller is.</param>
/// <param name="Email">The email of the account the token belongs to — a small convenience for the client, not used for auth itself.</param>
public record AuthResponse(
    string Token,
    string Email
);