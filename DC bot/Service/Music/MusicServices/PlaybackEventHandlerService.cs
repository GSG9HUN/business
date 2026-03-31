using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.MusicServices;

public class PlaybackEventHandlerService(
    IAudioService audioService,
    ILogger<PlaybackEventHandlerService> logger,
    ITrackEndedHandlerService trackEndedHandlerService) : IPlaybackEventHandlerService
{
    private readonly Dictionary<ulong, AsyncEventHandler<TrackEndedEventArgs>> _trackEndedHandlers = new();

    public void RegisterPlaybackFinishedHandler(ulong guildId, ILavalinkPlayer connection, IDiscordChannel textChannel)
    {
        if (_trackEndedHandlers.ContainsKey(guildId)) return;

        AsyncEventHandler<TrackEndedEventArgs> handler = async (_, args) =>
            await trackEndedHandlerService.HandleTrackEndedAsync(connection, args, textChannel);

        audioService.TrackEnded += handler;
        _trackEndedHandlers[guildId] = handler;
        logger.LogInformation("Playback finished event registered for guild {GuildId}", guildId);
    }

    public Task CleanupGuildAsync(ulong guildId)
    {
        try
        {
            if (!_trackEndedHandlers.TryGetValue(guildId, out var handler)) return Task.CompletedTask;

            audioService.TrackEnded -= handler;
            _trackEndedHandlers.Remove(guildId);
            logger.LogInformation("Cleaned up playback handler for guild {GuildId}", guildId);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during cleanup for guild {GuildId}", guildId);
            return Task.FromException(ex);
        }
    }
}