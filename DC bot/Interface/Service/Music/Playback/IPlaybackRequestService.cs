using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Playback.Models;
using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Interface.Service.Music.Playback;

public interface IPlaybackRequestService
{
    Task<PlaybackStartResult> PlayAsyncUrl(
        IDiscordChannel voiceStateChannel,
        Uri url,
        IDiscordMessage message,
        TrackSearchMode trackSearchMode,
        string? requestedBy = null);

    Task<PlaybackStartResult> PlayAsyncQuery(
        IDiscordChannel voiceStateChannel,
        string query,
        IDiscordMessage message,
        TrackSearchMode trackSearchMode,
        string? requestedBy = null);
}
