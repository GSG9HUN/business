using DC_bot.Interface.Discord;
using Lavalink4NET.Players;

namespace DC_bot.Interface.Service.Music.Lifecycle;

public interface IPlayerConnectionService
{
    Task<(ILavalinkPlayer? connection, IDiscordChannel? channel, ulong guildId, bool isValid)> TryJoinAndValidateAsync(
        IDiscordMessage message,
        IDiscordChannel? channel,
        CancellationToken cancellationToken = default);

    Task<(ILavalinkPlayer? connection, IDiscordChannel? channel, ulong guildId, bool isValid)>
        TryGetAndValidateExistingPlayerAsync(
            IDiscordMessage message,
            IDiscordChannel? channel,
            CancellationToken cancellationToken = default);

}