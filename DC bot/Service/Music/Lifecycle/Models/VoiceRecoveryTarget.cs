namespace DC_bot.Service.Music.Lifecycle.Models;

/// <summary>
/// Captures the player generation, voice channel, and node session permitted for gateway recovery.
/// </summary>
/// <param name="Session">The captured player generation whose ownership must remain current.</param>
/// <param name="ChannelId">The channel id supplied for this operation.</param>
/// <param name="NodeSessionId">The Lavalink node-session identity associated with the player.</param>
internal sealed record VoiceRecoveryTarget(PlaybackSession Session, ulong ChannelId, string NodeSessionId);
