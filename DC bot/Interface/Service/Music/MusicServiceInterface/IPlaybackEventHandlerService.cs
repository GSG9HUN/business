using DC_bot.Interface.Discord;
using Lavalink4NET.Players;

namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface IPlaybackEventHandlerService
{
    void RegisterPlaybackFinishedHandler(ulong guildId, ILavalinkPlayer connection, IDiscordChannel textChannel);
    Task CleanupGuildAsync(ulong guildId);
}