namespace NiWaterWatch.Api.Contracts;

/// <summary>
/// A river monitoring station, as exposed by the API.
/// Maps to a <see cref="NiWaterWatch.Domain.Entities.Station"/> entity, seeded from the
/// DAERA/NIEA open water quality dataset.
/// </summary>
/// <param name="Id">The station's database identifier.</param>
/// <param name="StationCode">The official DAERA station code (e.g. "UKGBNIF10014").</param>
/// <param name="Name">The human-readable location name.</param>
/// <param name="PrimaryBasin">The river basin the station belongs to (e.g. "Foyle (with Deele)").</param>
/// <param name="Easting">The station's Irish Grid easting coordinate, if known.</param>
/// <param name="Northing">The station's Irish Grid northing coordinate, if known.</param>
public record StationDto(
    int Id,
    string StationCode,
    string Name,
    string PrimaryBasin,
    int? Easting,
    int? Northing
);