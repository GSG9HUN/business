using DC_bot.Db;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.BotRuntimeStatus;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.Guilds;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.MobileAppUserSettings;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Persistence.Playlists;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Interface.Service.Persistence.Status;
using DC_bot.Repositories.BotControl;
using DC_bot.Repositories.BotRuntimeStatus;
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

namespace DC_bot.DependencyInjection;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        string postgresConnectionString)
    {
        return services
            .AddDbContextFactory<BotDbContext>(options => options.UseNpgsql(postgresConnectionString))
            .AddSingleton<IGuildDataRepository, GuildDataRepository>()
            .AddSingleton<IPlaybackStateRepository, PlaybackStateRepository>()
            .AddSingleton<IQueueRepository, QueueRepository>()
            .AddSingleton<IPlaylistRepository, PlaylistRepository>()
            .AddSingleton<IPlaylistTrackRepository, PlaylistTrackRepository>()
            .AddSingleton<IRepeatListRepository, RepeatListRepository>()
            .AddSingleton<IDbStatusCheck, DbStatusCheck>()
            .AddSingleton<IMobileAppUserRepository, MobileAppUserRepository>()
            .AddSingleton<IMobileAppSessionRepository, MobileAppSessionRepository>()
            .AddSingleton<IBotRuntimeStatusRepository, BotRuntimeStatusRepository>()
            .AddSingleton<IGuildBotStatusRepository, GuildBotStatusRepository>()
            .AddSingleton<IMobileAppUserSettingsRepository, MobileAppUserSettingsRepository>()
            .AddSingleton<IBotControlCommandsRepository, BotControlCommandsRepository>();
    }
}
