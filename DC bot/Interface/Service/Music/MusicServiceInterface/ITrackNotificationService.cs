using DC_bot.Interface.Discord;
using DSharpPlus;
using Lavalink4NET.Tracks;

namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface ITrackNotificationService
{
    public event Func<IDiscordChannel, DiscordClient, string, Task> TrackStarted;
    public Task NotifyNowPlayingAsync(IDiscordChannel textChannel, LavalinkTrack track);
    public Task NotifyQueueEmptyAsync(IDiscordChannel textChannel);
    public Task SendSafeAsync(IDiscordChannel channel, string message, string operation);
}