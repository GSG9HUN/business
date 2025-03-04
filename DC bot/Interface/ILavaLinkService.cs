using DSharpPlus;

namespace DC_bot.Interface;

public interface ILavaLinkService
{
    string GetCurrentTrack(ulong guildId);
    string GetCurrentTrackList(ulong guildId);
    Dictionary<ulong,bool> IsRepeating { get; set; }
    Dictionary<ulong,bool> IsRepeatingList { get; set; }
    Task PauseAsync(IDiscordChannel channel);
    Task PlayAsyncUrl(IDiscordChannel toDiscordChannel, Uri result, IDiscordChannel discordChannel);
    Task PlayAsyncQuery(IDiscordChannel toDiscordChannel, string query, IDiscordChannel discordChannel);
    Task ConnectAsync();
    Task SkipAsync(IDiscordChannel messageChannel);
    Task ResumeAsync(IDiscordChannel messageChannel);
    IReadOnlyCollection<ILavaLinkTrack> ViewQueue(ulong guildId);
    void CloneQueue(ulong guildId);
    void Init(ulong guildId);
    event Func<IDiscordChannel, DiscordClient, string, Task> TrackStarted;
}