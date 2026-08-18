namespace NiWaterWatch.Domain.Entities;

public class Station
{
    public int Id { get; set; }
    public string StationCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PrimaryBasin { get; set; } = string.Empty;
    public int? Easting { get; set; }
    public int? Northing { get; set; }

    public ICollection<Reading> Readings { get; set; } = new List<Reading>();
}