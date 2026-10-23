using DC_bot.Configuration;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup.DependencyInjection;

/// <summary>
/// Registers the lavalink runtime dependencies at the composition root.
/// </summary>
public static class LavalinkServiceCollectionExtensions
{

    /// <summary>
    /// Registers the lavalink runtime dependencies with the shared service collection.
    /// </summary>
    /// <param name="services">The services supplied for this operation.</param>
    /// <param name="lavalinkSettings">The lavalink settings supplied for this operation.</param>
    /// <returns>The service collection with these registrations, allowing further composition.</returns>
    public static IServiceCollection AddLavalinkRuntime(this IServiceCollection services, LavalinkSettings lavalinkSettings)
    {
        return services.ConfigureLavalink(options =>
        {
            var httpScheme = lavalinkSettings.Secured ? "https" : "http";
            var wsScheme = lavalinkSettings.Secured ? "wss" : "ws";
            options.BaseAddress = new Uri($"{httpScheme}://{lavalinkSettings.Hostname}:{lavalinkSettings.Port}");
            options.WebSocketUri = new Uri($"{wsScheme}://{lavalinkSettings.Hostname}:{lavalinkSettings.Port}/v4/websocket");
            options.Passphrase = lavalinkSettings.Password;
        })
            .AddLavalink();
    }
}
