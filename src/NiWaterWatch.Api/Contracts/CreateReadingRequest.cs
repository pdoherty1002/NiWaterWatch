using System.ComponentModel.DataAnnotations;

namespace NiWaterWatch.Api.Contracts;

/// <summary>What a registered user submits to add their own water quality reading for a station.</summary>
/// <param name="Date">The date the reading was taken.</param>
/// <param name="DissolvedOxygenMgL">The dissolved oxygen measurement, in mg/l.</param>
public record CreateReadingRequest(
    DateOnly Date,
    [Range(0, 50)] double DissolvedOxygenMgL
);