namespace NiWaterWatch.Api.Contracts;

/// <summary>
/// A single page of results, plus enough metadata for the caller to know whether
/// more pages exist. Generic so it can wrap any DTO type.
/// </summary>
/// <typeparam name="T">The type of item in this page (e.g. ReadingDto).</typeparam>
/// <param name="Items">The items on this page.</param>
/// <param name="Page">The current page number (1-based).</param>
/// <param name="PageSize">How many items per page.</param>
/// <param name="TotalCount">The total number of items across all pages.</param>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount
);