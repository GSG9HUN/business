namespace DC_bot.Interface.Service.Persistence.Models.Queue;

/// <summary>
/// Carries immutable queue item values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "Id">The id value used by this storage contract.</param>
/// <param name = "GuildId">The Discord guild whose persisted state is addressed.</param>
/// <param name = "Position">The position value used by this storage contract.</param>
/// <param name = "TrackIdentifier">The encoded source-track identity stored independently of any runtime player.</param>
/// <param name = "SourceQuery">The optional original reloadable URL or query, subject to the storage length limit.</param>
/// <param name = "SourceSearchMode">The optional persisted provider/search-mode name.</param>
/// <param name = "RequestedBy">The optional requester display value, subject to the storage length limit.</param>
/// <param name = "State">The state value used by this storage contract.</param>
/// <param name = "BatchId">The canonical playback batch identifier.</param>
/// <param name = "BatchItemId">The batch item id value used by this storage contract.</param>
/// <param name = "AddedAtUtc">The added at utc value used by this storage contract.</param>
/// <param name = "PlayedAtUtc">The played at utc value used by this storage contract.</param>
/// <param name = "SkippedAtUtc">The skipped at utc value used by this storage contract.</param>
public sealed record QueueItemRecord(
    long Id,
    ulong GuildId,
    int Position,
    string TrackIdentifier,
    string? SourceQuery,
    string? SourceSearchMode,
    string? RequestedBy,
    QueueItemState State,
    long? BatchId,
    long? BatchItemId,
    DateTimeOffset AddedAtUtc,
    DateTimeOffset? PlayedAtUtc,
    DateTimeOffset? SkippedAtUtc);
