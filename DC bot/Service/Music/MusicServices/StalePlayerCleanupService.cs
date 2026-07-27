using Lavalink4NET;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.MusicServices;

internal sealed class StalePlayerCleanupService(
    IAudioService audioService,
    ILogger<PlayerConnectionService> logger)
{
    internal async Task DisconnectBeforeJoinAsync(ulong guildId, CancellationToken cancellationToken)
    {
        var existingPlayer = await audioService.Players.GetPlayerAsync(guildId, cancellationToken).ConfigureAwait(false);
        if (existingPlayer is null || existingPlayer.ConnectionState.IsConnected)
        {
            return;
        }

        logger.LogWarning(
            "Disconnecting stale Lavalink player before join. Guild: {GuildId}, ConnectionState: {ConnectionState}, PlayerState: {PlayerState}, VoiceChannelId: {VoiceChannelId}",
            guildId,
            existingPlayer.ConnectionState,
            existingPlayer.State,
            existingPlayer.VoiceChannelId);

        await existingPlayer.DisconnectAsync(cancellationToken).ConfigureAwait(false);
    }
}
