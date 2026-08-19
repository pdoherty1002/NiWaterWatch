namespace NiWaterWatch.Domain.Entities;

/// <summary>
/// A single water quality reading at a station. Serves both official DAERA data
/// (imported in bulk) and readings submitted by registered users — distinguished
/// only by whether <see cref="UserId"/> is set.
/// </summary>
public class Reading
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Foreign key to the station this reading was taken at. Required.</summary>
    public int StationId { get; set; }

    /// <summary>Navigation property to the owning station.</summary>
    public Station Station { get; set; } = null!;

    /// <summary>The date the reading was taken.</summary>
    public DateOnly Date { get; set; }

    /// <summary>The dissolved oxygen measurement, in mg/l, if recorded.</summary>
    public double? DissolvedOxygenMgL { get; set; }

    /// <summary>
    /// The user who submitted this reading, if any.
    /// Null means this is official DAERA data — not stored as a separate flag,
    /// this nullability *is* the distinction.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>Navigation property to the submitting user, if any.</summary>
    public ApplicationUser? User { get; set; }

    /// <summary>When this row was created in the database.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}