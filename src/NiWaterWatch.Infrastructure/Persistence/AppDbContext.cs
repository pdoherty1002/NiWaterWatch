using Microsoft.EntityFrameworkCore;
using NiWaterWatch.Domain.Entities;

namespace NiWaterWatch.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Reading> Readings => Set<Reading>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasIndex(s => s.StationCode).IsUnique();
        });

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Reading>(entity =>
        {
            // Covers "readings for station X in date range Y" — the query
            // both the station detail page and the trend view will run constantly.
            entity.HasIndex(r => new { r.StationId, r.Date });

            entity.HasOne(r => r.Station)
                  .WithMany(s => s.Readings)
                  .HasForeignKey(r => r.StationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.User)
                  .WithMany(u => u.Readings)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        base.OnModelCreating(modelBuilder);
    }
}