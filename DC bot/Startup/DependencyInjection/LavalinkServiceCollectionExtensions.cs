using DC_bot.Configuration;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup.DependencyInjection;

public static class LavalinkServiceCollectionExtensions
{
    public static IServiceCollection AddLavalinkRuntime(
        this IServiceCollection services,
        LavalinkSettings lavalinkSettings)
    {
        return services
            .ConfigureLavalink(options =>
            {
                var httpScheme = lavalinkSettings.Secured ? "https" : "http";
                var wsScheme = lavalinkSettings.Secured ? "wss" : "ws";
                options.BaseAddress = new Uri($"{httpScheme}://{lavalinkSettings.Hostname}:{lavalinkSettings.Port}");
                options.WebSocketUri =
                    new Uri($"{wsScheme}://{lavalinkSettings.Hostname}:{lavalinkSettings.Port}/v4/websocket");
                options.Passphrase = lavalinkSettings.Password;
            })
            .AddLavalink();
    }
}