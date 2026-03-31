using DC_bot.Interface.Discord;
using Lavalink4NET.Players;

namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface IPlayerConnectionService
{
    Task<(ILavalinkPlayer? connection, IDiscordChannel? channel, ulong guildId, bool isValid)> TryJoinAndValidateAsync(
        IDiscordMessage message,
        IDiscordChannel? channel);

    Task<(ILavalinkPlayer? connection, IDiscordChannel? channel, ulong guildId, bool isValid)>
        TryGetAndValidateExistingPlayerAsync(
            IDiscordMessage message,
            IDiscordChannel? channel);
}