using Microsoft.EntityFrameworkCore;
using NiWaterWatch.Domain.Entities;

namespace NiWaterWatch.Infrastructure.Persistence;

/// <summary>
/// The EF Core database context for the whole application — the single point of contact
/// between the C# entities and the underlying PostgreSQL tables.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>The Stations table.</summary>
    public DbSet<Station> Stations => Set<Station>();

    /// <summary>The Readings table.</summary>
    public DbSet<Reading> Readings => Set<Reading>();

    /// <summary>The Users table.</summary>
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    /// <summary>
    /// Configures indexes, uniqueness constraints, and relationship/delete behavior
    /// using the Fluent API — kept here rather than as attributes on the entities, so
    /// the Domain project stays free of any EF Core dependency.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Station>(entity =>
        {
            // A station's code must be unique — prevents duplicate imports.
            entity.HasIndex(s => s.StationCode).IsUnique();
        });

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            // No two users can register with the same email.
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Reading>(entity =>
        {
            // Composite index matching the app's most common query: readings for a
            // given station within a date range.
            entity.HasIndex(r => new { r.StationId, r.Date });

            // Deleting a station deletes its readings — a reading pointing at a
            // nonexistent station is meaningless.
            entity.HasOne(r => r.Station)
                  .WithMany(s => s.Readings)
                  .HasForeignKey(r => r.StationId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Deleting a user detaches their readings (UserId becomes null) rather
            // than deleting the readings themselves — account deletion shouldn't
            // destroy historical water quality data.
            entity.HasOne(r => r.User)
                  .WithMany(u => u.Readings)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        base.OnModelCreating(modelBuilder);
    }
}