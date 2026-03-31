using DC_bot.Interface.Discord;
using Lavalink4NET.Players;
using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface ITrackPlaybackService
{
    Task PlayTheFoundMusicAsync(TrackLoadResult searchQuery, ILavalinkPlayer connection, IDiscordChannel textChannel);
    Task PlayTrackFromQueueAsync(ILavalinkPlayer player, IDiscordChannel textChannel);
    Task TryPlayNextTrackAsync(ILavalinkPlayer player, IDiscordChannel textChannel, ulong guildId);
}