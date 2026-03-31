using DC_bot.Interface.Discord;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface ITrackNotificationService
{
    public event Func<IDiscordChannel, DiscordClient, DiscordEmbed, Task> TrackStarted;

    public Task NotifyNowPlayingAsync(IDiscordChannel textChannel, ILavaLinkTrack track, TimeSpan position,
        TimeSpan duration);

    public Task NotifyQueueEmptyAsync(IDiscordChannel textChannel);
    public Task SendSafeAsync(IDiscordChannel channel, string message, string operation);
    DiscordEmbed BuildNowPlayingEmbed(ILavaLinkTrack track, TimeSpan pos, TimeSpan trackDuration);
}