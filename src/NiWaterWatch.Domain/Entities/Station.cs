namespace NiWaterWatch.Domain.Entities;

/// <summary>
/// A river monitoring station. Seeded from the DAERA/NIEA open water quality dataset —
/// not currently creatable by users, only by the import process.
/// </summary>
public class Station
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>The official DAERA station code (e.g. "UKGBNIF10014"). Unique.</summary>
    public string StationCode { get; set; } = string.Empty;

    /// <summary>Human-readable location name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The river basin this station belongs to (e.g. "Foyle (with Deele)").</summary>
    public string PrimaryBasin { get; set; } = string.Empty;

    /// <summary>Irish Grid easting coordinate, if known.</summary>
    public int? Easting { get; set; }

    /// <summary>Irish Grid northing coordinate, if known.</summary>
    public int? Northing { get; set; }

    /// <summary>All readings recorded at this station — both official DAERA data and user submissions.</summary>
    public ICollection<Reading> Readings { get; set; } = new List<Reading>();
}