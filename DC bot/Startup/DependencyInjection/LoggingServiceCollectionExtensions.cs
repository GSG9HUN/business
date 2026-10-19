using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.Startup.DependencyInjection;

public static class LoggingServiceCollectionExtensions
{
    public static IServiceCollection AddBotLogging(this IServiceCollection services)
    {
        return services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
                    options.UseUtcTimestamp = true;
                })
                .SetMinimumLevel(LogLevel.Warning)
                .AddFilter("DC_bot.Service.Core.ValidationService", LogLevel.Debug)
                .AddFilter("DC_bot.Wrapper.Discord.DiscordClientEventHandler", LogLevel.Information)
                .AddFilter("DC_bot.Service.Music.Lifecycle", LogLevel.Information)
                .AddFilter("DSharpPlus", LogLevel.Debug)
                .AddFilter("Lavalink4NET", LogLevel.Debug);
        });
    }
}