namespace DC_bot.BotControl.Payloads;

/// <summary>
/// Carries immutable playlist import command values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "Name">The stored guild-owned playlist name.</param>
/// <param name = "Url">The url value used by this storage contract.</param>
public sealed record PlaylistImportCommandPayload(string Name, string Url);
