using Moq;
using Xunit;
using NiWaterWatch.Api.Services;
using NiWaterWatch.Domain.Entities;
using NiWaterWatch.Domain.Interfaces;
using System.Linq.Expressions;

namespace NiWaterWatch.Tests.Services;

/// <summary>Tests for <see cref="StationService"/>, using a mocked repository. </summary>
public class StationServiceTests
{
    [Fact]
    public async Task GetByIdAsync_ReturnsStation_WhenStationExists()
    {
        // Arrange — set up a fake repository and tell it exactly what to return.
        var fakeStation = new Station
        {
            Id = 1,
            StationCode = "UKGBNIF10014",
            Name = "GLENMORNANRATCATHERINESBR",
            PrimaryBasin = "Foyle (with Deele)"
        };

        var mockRepo = new Mock<IRepository<Station, int>>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(fakeStation);

        var service = new StationService(mockRepo.Object);

        // Act — call the actual method being tested.
        var result = await service.GetByIdAsync(1);

        // Assert — check the result is what we expect.
        Assert.NotNull(result);
        Assert.Equal("UKGBNIF10014", result!.StationCode);
        Assert.Equal("GLENMORNANRATCATHERINESBR", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenStationDoesNotExist()
    {
        // Arrange — this time, tell the fake repository to return null, simulating
        // "no station with this Id exists."
        var mockRepo = new Mock<IRepository<Station, int>>();
        mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Station?)null);

        var service = new StationService(mockRepo.Object);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
public async Task SearchByNameAsync_ReturnsMatchingStations_MappedToDto()
{
    // Arrange — the fake repository doesn't need to actually filter anything;
    // it just needs to hand back whatever list we tell it to, regardless of
    // what predicate the service passes in.
    var fakeStations = new List<Station>
    {
        new Station { Id = 1, StationCode = "UKGBNIF10014", Name = "River Bann at Toome", PrimaryBasin = "Bann" },
        new Station { Id = 2, StationCode = "UKGBNIF10022", Name = "River Bann at Portglenone", PrimaryBasin = "Bann" }
    };

    var mockRepo = new Mock<IRepository<Station, int>>();
    mockRepo
        .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<Station, bool>>>()))
        .ReturnsAsync(fakeStations);

    var service = new StationService(mockRepo.Object);

    // Act
    var result = await service.SearchByNameAsync("Bann");

    // Assert
    Assert.Equal(2, result.Count);
    Assert.Equal("River Bann at Toome", result[0].Name);
    Assert.Equal("River Bann at Portglenone", result[1].Name);
}
}