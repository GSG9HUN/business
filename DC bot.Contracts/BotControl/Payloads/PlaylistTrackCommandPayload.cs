namespace DC_bot.BotControl.Payloads;

/// <summary>
/// Carries immutable playlist track command values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "PlaylistId">The durable playlist identifier.</param>
/// <param name = "PlaylistName">The stored guild-owned playlist name.</param>
/// <param name = "SongUrl">The song url value used by this storage contract.</param>
public sealed record PlaylistTrackCommandPayload(string PlaylistId, string PlaylistName, string SongUrl);
