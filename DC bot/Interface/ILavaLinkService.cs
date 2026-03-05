using DSharpPlus;
using Lavalink4NET.Players;
using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Interface;

public interface ILavaLinkService
{
    string GetCurrentTrack(ulong guildId);
    string GetCurrentTrackList(ulong guildId);
    Dictionary<ulong, bool> IsRepeating { get; set; }
    Dictionary<ulong, bool> IsRepeatingList { get; set; }
    Task PauseAsync(IDiscordMessage message, IDiscordMember? member);
    Task PlayAsyncUrl(IDiscordChannel toDiscordChannel, Uri result, IDiscordMessage message, TrackSearchMode trackSearchMode);
    Task PlayAsyncQuery(IDiscordChannel toDiscordChannel, string query, IDiscordMessage message, TrackSearchMode trackSearchMode);
    Task ConnectAsync();
    Task SkipAsync(IDiscordMessage message, IDiscordMember? member);
    Task ResumeAsync(IDiscordMessage message, IDiscordMember? member);
    IReadOnlyCollection<ILavaLinkTrack> ViewQueue(ulong guildId);
    void CloneQueue(ulong guildId);
    void Init(ulong guildId);
    event Func<IDiscordChannel, DiscordClient, string, Task> TrackStarted;
    Task StartPlayingQueue(IDiscordMessage message, IDiscordChannel textChannel, IDiscordMember? member);
    Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member);
    Task CleanupGuildAsync(ILavalinkPlayer connection, ulong guildId, IDiscordChannel textChannel);
}