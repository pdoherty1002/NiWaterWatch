using Microsoft.AspNetCore.Mvc;
using NiWaterWatch.Api.Services;

namespace NiWaterWatch.Api.Controllers;

/// <summary>Read-only endpoints for browsing river monitoring stations.</summary>
[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    // The service this controller delegates all actual logic to.
    private readonly StationService _stationService;

    /// <summary>Creates the controller, given a station service (supplied by dependency injection).</summary>
    public StationsController(StationService stationService)
    {
        _stationService = stationService;
    }

    /// <summary>Gets every monitoring station.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var stations = await _stationService.GetAllAsync();
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
}