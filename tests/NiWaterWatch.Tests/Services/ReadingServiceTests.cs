using Microsoft.EntityFrameworkCore;
using NiWaterWatch.Api.Services;
using NiWaterWatch.Domain.Entities;
using NiWaterWatch.Infrastructure.Persistence;
using NiWaterWatch.Api.Contracts;

namespace NiWaterWatch.Tests.Services;

public class ReadingServiceTests
{
    // Builds a fresh, isolated in-memory AppDbContext for a single test.
    // A new Guid per call means each test gets its own empty "database" —
    // nothing left over from any other test, no cleanup needed between them.
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetForStationAsync_ReturnsCorrectPage_OrderedByDateDescending()
    {
        // Arrange
        var context = CreateContext();

        var station = new Station
        {
            Id = 1,
            StationCode = "TEST01",
            Name = "Test Station",
            PrimaryBasin = "Test Basin"
        };
        context.Stations.Add(station);

        // Seed 5 readings for this station, on 5 different dates.
        for (int i = 1; i <= 5; i++)
        {
            context.Readings.Add(new Reading
            {
                Id = i,
                StationId = 1,
                Date = new DateOnly(2024, 1, i),
                DissolvedOxygenMgL = 8.0 + i
            });
        }
        await context.SaveChangesAsync();

        var service = new ReadingService(context);

        // Act — page 1, page size 2: should get the 2 most recent readings.
        var result = await service.GetForStationAsync(stationId: 1, page: 1, pageSize: 2);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(new DateOnly(2024, 1, 5), result.Items[0].Date); // most recent first
        Assert.Equal(new DateOnly(2024, 1, 4), result.Items[1].Date);
    }

    [Fact]
    public async Task GetForStationAsync_ReturnsEmptyPage_WhenStationHasNoReadings()
    {
        // Arrange — an empty in-memory database, no seeding at all.
        var context = CreateContext();
        var service = new ReadingService(context);

        // Act — station 99 doesn't exist anywhere in this database.
        var result = await service.GetForStationAsync(stationId: 99, page: 1, pageSize: 50);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
public async Task CreateAsync_SavesReading_WithCorrectUserIdAndIsUserSubmittedTrue()
{
    // Arrange
    var context = CreateContext();

    var station = new Station { Id = 1, StationCode = "TEST01", Name = "Test Station", PrimaryBasin = "Test Basin" };
    context.Stations.Add(station);
    await context.SaveChangesAsync();

    var service = new ReadingService(context);
    var userId = Guid.NewGuid();
    var request = new CreateReadingRequest(new DateOnly(2026, 8, 21), 8.5);

    // Act
    var result = await service.CreateAsync(stationId: 1, userId: userId, request: request);

    // Assert — check what the caller actually gets back.
    Assert.True(result.IsUserSubmitted);
    Assert.Equal(new DateOnly(2026, 8, 21), result.Date);
    Assert.Equal(8.5, result.DissolvedOxygenMgL);

    // Assert — separately confirm it was genuinely persisted, not just returned.
    var savedReading = await context.Readings.FirstOrDefaultAsync(r => r.Id == result.Id);
    Assert.NotNull(savedReading);
    Assert.Equal(userId, savedReading!.UserId);
    Assert.Equal(1, savedReading.StationId);
}
}