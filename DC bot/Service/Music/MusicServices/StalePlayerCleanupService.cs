using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using Lavalink4NET;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.MusicServices;

internal sealed class StalePlayerCleanupService(
    IAudioService audioService,
    IGuildBotStatusRepository guildBotStatusRepository,
    ILogger<PlayerConnectionService> logger)
{
    internal async Task DisconnectBeforeJoinAsync(ulong guildId, CancellationToken cancellationToken)
    {
        var existingPlayer =
            await audioService.Players.GetPlayerAsync(guildId, cancellationToken).ConfigureAwait(false);
        if (existingPlayer is null || existingPlayer.ConnectionState.IsConnected)
        {
            return;
        }

        // A disconnected snapshot can be temporary while Discord/Lavalink reconnects.
        // Allow a full player-update window before deciding this player is stale.
        for (var attempt = 0; attempt < 20; attempt++)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);
            var currentPlayer = await audioService.Players.GetPlayerAsync(guildId, cancellationToken)
                .ConfigureAwait(false);
            if (!ReferenceEquals(currentPlayer, existingPlayer) || existingPlayer.ConnectionState.IsConnected)
            {
                return;
            }
        }

        logger.LogWarning(
            "Disconnecting stale Lavalink player before join. Guild: {GuildId}, ConnectionState: {ConnectionState}, PlayerState: {PlayerState}, VoiceChannelId: {VoiceChannelId}",
            guildId,
            existingPlayer.ConnectionState,
            existingPlayer.State,
            existingPlayer.VoiceChannelId);

        await existingPlayer.DisconnectAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await guildBotStatusRepository.MarkDisconnectedVoiceAsync(guildId, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Stale player disconnected but status persistence failed. Guild: {GuildId}", guildId);
        }
    }
}