using DC_bot.Interface.Discord;
using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Interface.Service.Music;

public interface IPlaybackRequestService
{
    Task PlayAsyncUrl(IDiscordChannel voiceStateChannel, Uri url, IDiscordMessage message,
        TrackSearchMode trackSearchMode);

    Task PlayAsyncQuery(IDiscordChannel voiceStateChannel, string query, IDiscordMessage message,
        TrackSearchMode trackSearchMode);
}
