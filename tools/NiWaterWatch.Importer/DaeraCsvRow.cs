using CsvHelper.Configuration.Attributes;

namespace NiWaterWatch.Importer;

public class DaeraCsvRow
{
    [Name("StationCode")]
    public string StationCode { get; set; } = string.Empty;

    [Name("Location")]
    public string Location { get; set; } = string.Empty;

    [Name("PrimaryBasin")]
    public string PrimaryBasin { get; set; } = string.Empty;

    [Name("Easting")]
    public int? Easting { get; set; }

    [Name("Northing")]
    public int? Northing { get; set; }

    [Name("Date")]
    public string Date { get; set; } = string.Empty;

    [Name("DO_mg_l_")]
    public double? DissolvedOxygenMgL { get; set; }
}