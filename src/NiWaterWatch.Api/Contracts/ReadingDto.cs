namespace NiWaterWatch.Api.Contracts;

/// <summary>
/// A single water quality reading, as exposed by the API.
/// Maps to a <see cref="NiWaterWatch.Domain.Entities.Reading"/> entity — may originate from
/// the official DAERA dataset or from a registered user's own submission.
/// </summary>
/// <param name="Id">The reading's database identifier.</param>
/// <param name="Date">The date the reading was taken.</param>
/// <param name="DissolvedOxygenMgL">The dissolved oxygen measurement, in mg/l, if recorded.</param>
/// <param name="IsUserSubmitted">True if a registered user submitted this reading; false if it's official DAERA data.</param>
public record ReadingDto(
    int Id,
    DateOnly Date,
    double? DissolvedOxygenMgL,
    bool IsUserSubmitted
);