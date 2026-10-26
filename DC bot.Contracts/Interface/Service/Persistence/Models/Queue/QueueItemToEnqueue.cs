namespace DC_bot.Interface.Service.Persistence.Models.Queue;

/// <summary>
/// Carries immutable queue item to enqueue values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "TrackIdentifier">The encoded source-track identity stored independently of any runtime player.</param>
/// <param name = "SourceQuery">The optional original reloadable URL or query, subject to the storage length limit.</param>
/// <param name = "SourceSearchMode">The optional persisted provider/search-mode name.</param>
/// <param name = "RequestedBy">The optional requester display value, subject to the storage length limit.</param>
public sealed record QueueItemToEnqueue(
    string TrackIdentifier,
    string? SourceQuery,
    string? SourceSearchMode,
    string? RequestedBy);
