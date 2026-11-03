namespace DC_bot.Interface.Service.Persistence.Models.Playback;

/// <summary>
/// Carries immutable playback state values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "GuildId">The Discord guild whose persisted state is addressed.</param>
/// <param name = "IsRepeating">Whether single-track repetition is enabled.</param>
/// <param name = "IsRepeatingList">Whether canonical-list repetition is enabled.</param>
/// <param name = "CurrentTrackIdentifier">The current track identifier value used by this storage contract.</param>
/// <param name = "QueueItemId">The durable queue-item identifier, when a specific item is required.</param>
/// <param name = "UpdatedAtUtc">The updated at utc value used by this storage contract.</param>
/// <param name = "IsPaused">The is paused value used by this storage contract.</param>
/// <param name = "PositionSeconds">The position seconds value used by this storage contract.</param>
/// <param name = "PositionUpdatedAtUtc">The position updated at utc value used by this storage contract.</param>
public record PlaybackStateRecord(
    ulong GuildId,
    bool IsRepeating,
    bool IsRepeatingList,
    string? CurrentTrackIdentifier,
    long? QueueItemId,
    DateTimeOffset UpdatedAtUtc,
    bool IsPaused = false,
    int PositionSeconds = 0,
    DateTimeOffset? PositionUpdatedAtUtc = null);
