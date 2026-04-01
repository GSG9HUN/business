namespace DC_bot.Interface.Service.Persistence;

public interface IGuildDataRepository
{
    Task EnsureGuildExistsAsync(ulong guildId, CancellationToken cancellationToken = default);

    Task<bool> IsPremiumAsync(ulong guildId, CancellationToken cancellationToken = default);

    Task UpsertPremiumAsync(
        ulong guildId,
        bool isPremium,
        DateTimeOffset? premiumUntilUtc,
        CancellationToken cancellationToken = default);
}
