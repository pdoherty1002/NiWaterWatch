namespace NiWaterWatch.Domain.Entities;

public class Reading
{
    public int Id { get; set; }

    public int StationId { get; set; }
    public Station Station { get; set; } = null!;

    public DateOnly Date { get; set; }
    public double? DissolvedOxygenMgL { get; set; }

    // null = official DAERA reading, set = submitted by a user
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}