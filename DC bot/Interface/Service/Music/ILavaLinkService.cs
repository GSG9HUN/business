using DC_bot.Interface.Discord;
using DSharpPlus;
using DSharpPlus.Entities;
using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Interface.Service.Music;

public interface ILavaLinkService
{
    Task PauseAsync(IDiscordMessage message, IDiscordMember? member);

    Task PlayAsyncUrl(IDiscordChannel toDiscordChannel, Uri result, IDiscordMessage message,
        TrackSearchMode trackSearchMode);

    Task PlayAsyncQuery(IDiscordChannel toDiscordChannel, string query, IDiscordMessage message,
        TrackSearchMode trackSearchMode);

    Task ConnectAsync();
    Task SkipAsync(IDiscordMessage message, IDiscordMember? member);
    Task ResumeAsync(IDiscordMessage message, IDiscordMember? member);
    Task Init(ulong guildId);
    event Func<IDiscordChannel, DiscordClient, DiscordEmbed, Task> TrackStarted;
    Task StartPlayingQueue(IDiscordMessage message, IDiscordChannel textChannel, IDiscordMember? member);
    Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member);
}