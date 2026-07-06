using DC_bot.Interface.Service.Persistence;
using DC_bot.Persistence.Db;
using DC_bot.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup.DependencyInjection;

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
            .AddSingleton<IRepeatListRepository, RepeatListRepository>();
    }
}