using Microsoft.AspNetCore.Mvc;
using NiWaterWatch.Api.Services;

namespace NiWaterWatch.Api.Controllers;

/// <summary>Read-only endpoints for browsing river monitoring stations and their readings.</summary>
[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    private readonly StationService _stationService;
    private readonly ReadingService _readingService;

    /// <summary>Creates the controller, given its services (supplied by dependency injection).</summary>
    public StationsController(StationService stationService, ReadingService readingService)
    {
        _stationService = stationService;
        _readingService = readingService;
    }

    /// <summary>Gets every monitoring station.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var stations = await _stationService.GetAllAsync();
        return Ok(stations);
    }

    /// <summary>Searches for stations whose name contains the given text.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchByName([FromQuery] string name)
    {
        var stations = await _stationService.SearchByNameAsync(name);
        return Ok(stations);
    }

    /// <summary>Gets a single station by its Id.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var station = await _stationService.GetByIdAsync(id);

        if (station is null)
            return NotFound();

        return Ok(station);
    }

    /// <summary>Gets one page of readings for a given station, most recent first.</summary>
    /// <param name="id">The station's Id.</param>
    /// <param name="page">The page number to fetch. Defaults to 1.</param>
    /// <param name="pageSize">How many readings per page. Defaults to 50, capped at 200.</param>
    [HttpGet("{id}/readings")]
    public async Task<IActionResult> GetReadings(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var station = await _stationService.GetByIdAsync(id);

        if (station is null)
            return NotFound();

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 50;

        var readings = await _readingService.GetForStationAsync(id, page, pageSize);
        return Ok(readings);
    }
}