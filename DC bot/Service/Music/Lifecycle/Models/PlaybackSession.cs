using Lavalink4NET.Players;

namespace DC_bot.Service.Music.Lifecycle.Models;

/// <summary>
/// Identifies the guild player generation used to reject superseded playback work.
/// </summary>
/// <param name="GuildId">The Discord guild whose state or operation is addressed.</param>
/// <param name="Generation">The guild lifecycle generation used to reject stale work.</param>
/// <param name="Player">The lavalink player dependency used by this boundary.</param>
public sealed record PlaybackSession(ulong GuildId, long Generation, ILavalinkPlayer Player);
