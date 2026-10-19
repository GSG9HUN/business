using DC_bot.Interface.Discord;
using DC_bot.Service.Music.Lifecycle;
using Lavalink4NET.Events.Players;

namespace DC_bot.Interface.Service.Music.Events;

public interface ITrackEndedHandlerService
{
    Task HandleTrackEndedAsync(
        PlaybackSession session, Guid operationId, long queueItemId,
        TrackEndedEventArgs args, IDiscordChannel textChannel);
}