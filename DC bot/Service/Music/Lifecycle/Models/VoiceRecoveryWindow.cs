namespace DC_bot.Service.Music.Lifecycle.Models;

/// <summary>
/// Limits automatic voice recovery to one player generation and expiry time.
/// </summary>
/// <param name="Generation">The guild lifecycle generation used to reject stale work.</param>
/// <param name="Until">The until supplied for this operation.</param>
internal sealed record VoiceRecoveryWindow(long Generation, DateTimeOffset Until);
