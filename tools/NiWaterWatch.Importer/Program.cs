using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using NiWaterWatch.Importer;
using Microsoft.EntityFrameworkCore;
using NiWaterWatch.Infrastructure.Persistence;
using NiWaterWatch.Domain.Entities;

// ===== 1. Read the CSV =====

// Full path to the CSV file, built relative to where the compiled .exe actually runs
// (bin/Debug/...), not relative to where Program.cs lives on disk.
var csvPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Data", "dissolved-oxygen.csv");

// CsvHelper settings. MissingFieldFound = null means "don't throw if a row is missing
// a column we expected" — a safety net for real-world government data.
var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    MissingFieldFound = null
};

// Opens the CSV file as a stream of text.
using var reader = new StreamReader(csvPath);

// Wraps the stream reader with CsvHelper's parser, using the config above.
using var csv = new CsvReader(reader, config);

// Reads every row into a DaeraCsvRow object (defined in DaeraCsvRow.cs) and loads them all
// into memory as a List — the full 151,037-row dataset, held in RAM for the rest of the run.
var rows = csv.GetRecords<DaeraCsvRow>().ToList();

Console.WriteLine($"Total rows:          {rows.Count}");
Console.WriteLine($"Distinct stations:   {rows.Select(r => r.StationCode).Distinct().Count()}");

// Every date that parsed successfully — used further down to report the overall date range.
var parsedDates = new List<DateOnly>();

// How many rows had a Date value that didn't match the expected yyyy/MM/dd format.
var badDateRows = 0;

foreach (var row in rows)
{
    // Tries to parse this row's Date string into a real DateOnly value. TryParseExact
    // returns true/false instead of throwing, so one bad row doesn't crash the whole loop.
    if (DateOnly.TryParseExact(row.Date, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        parsedDates.Add(date);
    else
        badDateRows++;
}

Console.WriteLine($"Unparseable dates:   {badDateRows}");
if (parsedDates.Count > 0)
    Console.WriteLine($"Date range:          {parsedDates.Min()} to {parsedDates.Max()}");

// Every dissolved oxygen value that actually has a number (skips rows with no DO recorded).
var doValues = rows.Where(r => r.DissolvedOxygenMgL.HasValue).Select(r => r.DissolvedOxygenMgL!.Value).ToList();

// How many rows had no DO value at all.
var missingDo = rows.Count - doValues.Count;

Console.WriteLine($"Missing DO readings: {missingDo} ({missingDo * 100.0 / rows.Count:F1}%)");
if (doValues.Count > 0)
    Console.WriteLine($"DO min / avg / max:  {doValues.Min():F2} / {doValues.Average():F2} / {doValues.Max():F2}");

Console.WriteLine("Distribution of high readings:");
// Checks how many readings exceed each threshold, to see how the "suspicious" tail of the
// data drops off rather than picking one cutoff blindly.
foreach (var t in new[] { 15, 17, 20, 25, 30, 35 })
{
    var count = doValues.Count(v => v > t);
    Console.WriteLine($"  Above {t} mg/l: {count} ({count * 100.0 / doValues.Count:F3}%)");
}

// All DO values sorted smallest to largest — needed to calculate percentiles below.
var sorted = doValues.OrderBy(v => v).ToList();

// Local function: given a percentile (e.g. 99), returns the value below which that
// percentage of all readings fall. Used to find where "normal" data actually ends.
double Percentile(double p)
{
    var idx = (int)Math.Ceiling(p / 100.0 * sorted.Count) - 1;
    return sorted[Math.Clamp(idx, 0, sorted.Count - 1)];
}
Console.WriteLine($"99th percentile:   {Percentile(99):F2}");
Console.WriteLine($"99.9th percentile: {Percentile(99.9):F2}");

// How many station+date combinations appear more than once — i.e. two readings logged
// for the same station on the same day.
var duplicates = rows.GroupBy(r => (r.StationCode, r.Date)).Count(g => g.Count() > 1);
Console.WriteLine($"Duplicate station+date rows: {duplicates}");

// ===== 2. Connect to the database =====

// Local Postgres connection details. Hardcoded deliberately — this is a one-off tool run
// by hand on your own machine, not a hosted app, so it's not a secret worth protecting.
const string connectionString = "Host=localhost;Database=niwaterwatch;Username=postgres;Password=postgres";

// The configuration EF Core needs to talk to Postgres — the manual equivalent of the
// AddDbContext(...) call in the API's Program.cs, since there's no ASP.NET Core hosting
// here to wire it up automatically.
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(connectionString)
    .Options;

// The actual EF Core context — the same AppDbContext class used everywhere else in the
// solution, just constructed by hand here instead of injected by the framework.
using var context = new AppDbContext(options);

// Simple connectivity check before doing anything real.
var canConnect = await context.Database.CanConnectAsync();
Console.WriteLine($"Database reachable: {canConnect}");

// ===== 3. Build and save Stations =====

// Repository for the Station entity — the same generic Repository<T, TKey> class from
// Infrastructure/Persistence/Repository.cs.
var stationRepo = new Repository<Station, int>(context);

// Every StationCode already sitting in the database. Used to avoid inserting duplicates
// if this program gets run more than once.
var existingCodes = (await stationRepo.GetAllAsync())
    .Select(s => s.StationCode)
    .ToHashSet();

// Builds the list of new Station entities to insert: groups all rows down to one group
// per distinct StationCode, skips codes already in the database, takes the first CSV row
// from each remaining group, and maps it into a real Station entity.
var newStations = rows
    .GroupBy(r => r.StationCode)
    .Where(g => !existingCodes.Contains(g.Key))
    .Select(g => g.First())
    .Select(r => new Station
    {
        StationCode = r.StationCode,
        Name = r.Location,
        PrimaryBasin = r.PrimaryBasin,
        Easting = r.Easting,
        Northing = r.Northing
    })
    .ToList();

Console.WriteLine($"New stations to add: {newStations.Count}");

// Stages each new station for insertion (doesn't hit the database yet).
foreach (var station in newStations)
    await stationRepo.AddAsync(station);

// Actually writes all the staged stations to Postgres in one go.
await stationRepo.SaveChangesAsync();
Console.WriteLine("Stations saved.");

// ===== 4. Build and save Readings =====

// How many Reading rows already exist — used as a simple "have we already done this?" check.
var readingCount = await context.Readings.CountAsync();

if (readingCount > 0)
{
    Console.WriteLine($"Readings table already has {readingCount} rows — skipping import to avoid duplicates.");
}
else
{
    // Repository for the Reading entity.
    var readingRepo = new Repository<Reading, int>(context);

    // Maps each station's code to its real database Id, so each Reading can be linked to
    // the correct Station via a foreign key (the CSV only has the code, not the Id).
    var stationIdLookup = (await stationRepo.GetAllAsync())
        .ToDictionary(s => s.StationCode, s => s.Id);

    // How many Reading entities to accumulate before saving as one batch, rather than one
    // row at a time (slow) or all 151,037 at once (memory-heavy).
    const int batchSize = 2000;

    // The current batch of Reading entities waiting to be saved.
    var batch = new List<Reading>(batchSize);

    // Running total of how many readings have actually been written to the database so far.
    var totalSaved = 0;

    // Running total of how many readings had a DO value above 20 mg/l — the threshold agreed
    // on for flagging physically implausible readings without excluding them.
    var suspiciousCount = 0;

    foreach (var row in rows)
    {
        // If this row's station code somehow isn't in the lookup (shouldn't happen, but
        // guards against it), skip the row rather than crash.
        if (!stationIdLookup.TryGetValue(row.StationCode, out var stationId))
            continue;

        // Parses this row's date string into a real DateOnly value. Safe to use ParseExact
        // (which throws on failure) here, since the exploration pass already confirmed zero
        // unparseable dates in this file.
        var date = DateOnly.ParseExact(row.Date, "yyyy/MM/dd", CultureInfo.InvariantCulture);

        // Logs a warning for any reading above the 20 mg/l threshold, without skipping it —
        // matches the "import everything, but make the questionable ones easy to find" decision.
        if (row.DissolvedOxygenMgL is > 20)
        {
            suspiciousCount++;
            Console.WriteLine($"  [suspicious] {row.StationCode} | {date} | DO={row.DissolvedOxygenMgL}");
        }

        // Builds the actual Reading entity for this row. UserId is explicitly null — every
        // row from this import is official DAERA data, not a user submission.
        batch.Add(new Reading
        {
            StationId = stationId,
            Date = date,
            DissolvedOxygenMgL = row.DissolvedOxygenMgL,
            UserId = null
        });

        // Once the batch reaches batchSize, save it and clear EF Core's tracked-entity
        // memory before the next batch — keeps memory usage flat across all 151,037 rows.
        if (batch.Count == batchSize)
        {
            foreach (var r in batch)
                await readingRepo.AddAsync(r);
            await readingRepo.SaveChangesAsync();
            context.ChangeTracker.Clear();

            totalSaved += batch.Count;
            batch.Clear();
            Console.WriteLine($"  Saved {totalSaved} / {rows.Count} readings...");
        }
    }

    // Saves whatever's left after the loop ends — the final partial batch that didn't
    // reach batchSize (e.g. the last 1,037 rows, since the total isn't a clean multiple of 2000).
    if (batch.Count > 0)
    {
        foreach (var r in batch)
            await readingRepo.AddAsync(r);
        await readingRepo.SaveChangesAsync();
        totalSaved += batch.Count;
    }

    Console.WriteLine($"Readings saved: {totalSaved}");
    Console.WriteLine($"Suspicious (>20 mg/l) readings flagged: {suspiciousCount}");
}