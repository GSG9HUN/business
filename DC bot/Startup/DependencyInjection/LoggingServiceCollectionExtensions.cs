using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.Startup.DependencyInjection;

public static class LoggingServiceCollectionExtensions
{
    public static IServiceCollection AddBotLogging(this IServiceCollection services)
    {
        return services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options => options.IncludeScopes = true)
                .SetMinimumLevel(LogLevel.Warning) 
                .AddFilter("DC_bot.Service.Core.ValidationService", LogLevel.Debug)
                .AddFilter("Lavalink4NET", LogLevel.Debug);
        });
    }
}
