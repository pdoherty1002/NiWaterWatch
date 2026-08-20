using NiWaterWatch.Api.Contracts;
using NiWaterWatch.Domain.Entities;
using NiWaterWatch.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace NiWaterWatch.Api.Services;

/// <summary>
/// Read logic for stations — fetches <see cref="Station"/> entities and maps them
/// to the API's public <see cref="StationDto"/> shape.
/// </summary>
public class StationService
{
    private readonly IRepository<Station, int> _stationRepo;

    /// <summary>Creates the service, given a station repository (supplied by dependency injection).</summary>
    public StationService(IRepository<Station, int> stationRepo)
    {
        _stationRepo = stationRepo;
    }

    /// <summary>Fetches every station, mapped to its public DTO shape.</summary>
    public async Task<IReadOnlyList<StationDto>> GetAllAsync()
    {
        var stations = await _stationRepo.GetAllAsync();

        return stations
            .Select(s => new StationDto(s.Id, s.StationCode, s.Name, s.PrimaryBasin, s.Easting, s.Northing))
            .ToList();
    }

    /// <summary>Fetches a single station by its database Id, or null if not found.</summary>
    public async Task<StationDto?> GetByIdAsync(int id)
    {
        var station = await _stationRepo.GetByIdAsync(id);

        if (station is null)
            return null;

        return new StationDto(station.Id, station.StationCode, station.Name, station.PrimaryBasin, station.Easting, station.Northing);
    }

    /// <summary>Searches for stations whose name contains the given text.</summary>
    public async Task<IReadOnlyList<StationDto>> SearchByNameAsync(string name)
    {
        var stations = await _stationRepo.GetByConditionAsync(s => EF.Functions.ILike(s.Name, $"%{name}%"));

        return stations
            .Select(s => new StationDto(s.Id, s.StationCode, s.Name, s.PrimaryBasin, s.Easting, s.Northing))
            .ToList();
    }
}