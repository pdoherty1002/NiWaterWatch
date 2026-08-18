using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using NiWaterWatch.Importer;

var csvPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Data", "dissolved-oxygen.csv");

var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    MissingFieldFound = null
};

using var reader = new StreamReader(csvPath);
using var csv = new CsvReader(reader, config);

var rows = csv.GetRecords<DaeraCsvRow>().ToList();

Console.WriteLine($"Total rows:          {rows.Count}");
Console.WriteLine($"Distinct stations:   {rows.Select(r => r.StationCode).Distinct().Count()}");

var parsedDates = new List<DateOnly>();
var badDateRows = 0;
foreach (var row in rows)
{
    if (DateOnly.TryParseExact(row.Date, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        parsedDates.Add(date);
    else
        badDateRows++;
}

Console.WriteLine($"Unparseable dates:   {badDateRows}");
if (parsedDates.Count > 0)
    Console.WriteLine($"Date range:          {parsedDates.Min()} to {parsedDates.Max()}");

var doValues = rows.Where(r => r.DissolvedOxygenMgL.HasValue).Select(r => r.DissolvedOxygenMgL!.Value).ToList();
var missingDo = rows.Count - doValues.Count;
Console.WriteLine($"Missing DO readings: {missingDo} ({missingDo * 100.0 / rows.Count:F1}%)");
if (doValues.Count > 0)
    Console.WriteLine($"DO min / avg / max:  {doValues.Min():F2} / {doValues.Average():F2} / {doValues.Max():F2}");


Console.WriteLine("Distribution of high readings:");
foreach (var t in new[] { 15, 17, 20, 25, 30, 35 })
{
    var count = doValues.Count(v => v > t);
    Console.WriteLine($"  Above {t} mg/l: {count} ({count * 100.0 / doValues.Count:F3}%)");
}

var sorted = doValues.OrderBy(v => v).ToList();
double Percentile(double p)
{
    var idx = (int)Math.Ceiling(p / 100.0 * sorted.Count) - 1;
    return sorted[Math.Clamp(idx, 0, sorted.Count - 1)];
}
Console.WriteLine($"99th percentile:   {Percentile(99):F2}");
Console.WriteLine($"99.9th percentile: {Percentile(99.9):F2}");
var duplicates = rows.GroupBy(r => (r.StationCode, r.Date)).Count(g => g.Count() > 1);
Console.WriteLine($"Duplicate station+date rows: {duplicates}");