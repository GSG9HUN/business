namespace DC_bot.Interface.Service.Persistence.Models.Queue;

/// <summary>
/// Carries immutable playback batch values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "Id">The id value used by this storage contract.</param>
/// <param name = "CreatedAtUtc">The created at utc value used by this storage contract.</param>
/// <param name = "CompletedAtUtc">The completed at utc value used by this storage contract.</param>
/// <param name = "TrackCount">The track count value used by this storage contract.</param>
public sealed record PlaybackBatchRecord(long Id, DateTimeOffset CreatedAtUtc, DateTimeOffset? CompletedAtUtc, int TrackCount);
