namespace DC_bot.BotControl.Payloads;

/// <summary>
/// Carries immutable playlist command values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "PlaylistId">The durable playlist identifier.</param>
/// <param name = "PlaylistName">The stored guild-owned playlist name.</param>
public sealed record PlaylistCommandPayload(string PlaylistId, string PlaylistName);
