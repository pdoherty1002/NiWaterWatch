using Microsoft.EntityFrameworkCore;
using NiWaterWatch.Api.Contracts;
using NiWaterWatch.Domain.Entities;
using NiWaterWatch.Infrastructure.Persistence;

namespace NiWaterWatch.Api.Services;

/// <summary>
/// Read logic for readings — fetches <see cref="Reading"/> entities for a given
/// station, paginated, and maps them to the API's public <see cref="ReadingDto"/> shape.
/// </summary>
public class ReadingService
{
    // Queried directly through AppDbContext rather than IRepository, since paging
    // needs Skip/Take/OrderBy composed directly onto the query before it runs —
    // the generic repository's GetAllAsync() only returns a full, unfiltered list.
    private readonly AppDbContext _context;

    /// <summary>Creates the service, given the database context (supplied by dependency injection).</summary>
    public ReadingService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Fetches one page of readings for a given station, most recent first.
    /// </summary>
    /// <param name="stationId">The station to fetch readings for.</param>
    /// <param name="page">The page number to fetch (1-based).</param>
    /// <param name="pageSize">How many readings per page.</param>
    public async Task<PagedResult<ReadingDto>> GetForStationAsync(int stationId, int page, int pageSize)
    {
        var query = _context.Readings
            .Where(r => r.StationId == stationId)
            .OrderByDescending(r => r.Date);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReadingDto(r.Id, r.Date, r.DissolvedOxygenMgL, r.UserId != null))
            .ToListAsync();

        return new PagedResult<ReadingDto>(items, page, pageSize, totalCount);
    }

    /// <summary>
    /// Creates a new reading for a station, submitted by a registered user.
    /// </summary>
    /// <param name="stationId">The station this reading belongs to.</param>
    /// <param name="userId">The Id of the user submitting it — taken from their token, never from the request body.</param>
    /// <param name="request">The reading data the user submitted.</param>
    public async Task<ReadingDto> CreateAsync(int stationId, Guid userId, CreateReadingRequest request)
    {
        var reading = new Reading
        {
            StationId = stationId,
            Date = request.Date,
            DissolvedOxygenMgL = request.DissolvedOxygenMgL,
            UserId = userId
        };

        _context.Readings.Add(reading);
        await _context.SaveChangesAsync();

        return new ReadingDto(reading.Id, reading.Date, reading.DissolvedOxygenMgL, reading.UserId != null);
    }
}