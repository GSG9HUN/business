using DC_bot.Interface.Discord;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;

namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface ITrackEndedHandlerService
{
    Task HandleTrackEndedAsync(ILavalinkPlayer player, TrackEndedEventArgs args, IDiscordChannel textChannel);
}