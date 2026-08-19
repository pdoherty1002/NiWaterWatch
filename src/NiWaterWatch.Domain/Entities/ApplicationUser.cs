namespace NiWaterWatch.Domain.Entities;

/// <summary>A registered user, able to submit their own water quality readings.</summary>
public class ApplicationUser
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The user's email address. Unique — also used as their login identifier.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Hashed password. Never stored in plaintext.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>When this account was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>All readings this user has submitted.</summary>
    public ICollection<Reading> Readings { get; set; } = new List<Reading>();
}