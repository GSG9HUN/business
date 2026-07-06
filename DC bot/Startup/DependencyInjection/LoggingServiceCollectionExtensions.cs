using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.Startup.DependencyInjection;

public static class LoggingServiceCollectionExtensions
{
    public static IServiceCollection AddBotLogging(this IServiceCollection services)
    {
        return services.AddLogging(builder => { builder.AddConsole().SetMinimumLevel(LogLevel.Debug); });
    }
}