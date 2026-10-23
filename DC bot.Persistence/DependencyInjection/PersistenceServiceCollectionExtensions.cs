using DC_bot.Db;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.Guilds;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.MobileAppUserSettings;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Persistence.Playlists;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Interface.Service.Persistence.Status;
using DC_bot.Repositories.BotControl;
using DC_bot.Repositories.GuildBotStatus;
using DC_bot.Repositories.Guilds;
using DC_bot.Repositories.MobileApps;
using DC_bot.Repositories.MobileAppUserSettings;
using DC_bot.Repositories.Playback;
using DC_bot.Repositories.Playlists;
using DC_bot.Repositories.Queue;
using DC_bot.Repositories.Status;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.DependencyInjection;

/// <summary>
/// Composes shared PostgreSQL contexts and repository dependencies for the bot and API.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{

    /// <summary>
    /// Registers PostgreSQL contexts, repository helpers, and shared repository contracts with their runtime lifetimes.
    /// </summary>
    /// <param name = "services">The service collection or provider used at the persistence composition boundary.</param>
    /// <param name = "postgresConnectionString">The PostgreSQL connection string used by the context or dedicated listening connection.</param>
    /// <returns>The mapped, normalized, or configured value described above.</returns>
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, string postgresConnectionString)
    {
        return services.AddDbContextFactory<BotDbContext>(options => options.UseNpgsql(postgresConnectionString))
            .AddSingleton<IGuildDataRepository, GuildDataRepository>()
            .AddSingleton<IPlaybackStateRepository, PlaybackStateRepository>()
            .AddSingleton<QueueClaimService>()
            .AddSingleton<QueueBatchRepository>()
            .AddSingleton<IQueueRepository>(provider => new QueueRepository(provider.GetRequiredService<IDbContextFactory<BotDbContext>>(), provider.GetRequiredService<QueueClaimService>(), provider.GetRequiredService<QueueBatchRepository>()))
            .AddSingleton<IPlaylistRepository, PlaylistRepository>()
            .AddSingleton<IPlaylistTrackRepository, PlaylistTrackRepository>()
            .AddSingleton<IRepeatListRepository, RepeatListRepository>()
            .AddSingleton<IDbStatusCheck, DbStatusCheck>()
            .AddSingleton<IMobileAppUserRepository, MobileAppUserRepository>()
            .AddSingleton<IMobileRealtimeMembershipRepository, MobileRealtimeMembershipRepository>()
            .AddSingleton<IMobileAppSessionRepository, MobileAppSessionRepository>()
            .AddSingleton<IGuildBotStatusRepository, GuildBotStatusRepository>()
            .AddSingleton<IMobileAppUserSettingsRepository, MobileAppUserSettingsRepository>()
            .AddSingleton<IBotControlCommandNotifier>(provider => new PostgreSqlBotControlCommandNotifier(postgresConnectionString, provider.GetRequiredService<ILogger<PostgreSqlBotControlCommandNotifier>>()))
            .AddSingleton<IBotControlCommandsRepository, BotControlCommandsRepository>();
    }
}
