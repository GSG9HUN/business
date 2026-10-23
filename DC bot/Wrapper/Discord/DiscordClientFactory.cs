using DC_bot.Configuration;
using DSharpPlus;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper.Discord;

/// <summary>
/// Creates the Discord client with configured credentials, intents, and event handling.
/// </summary>
public static class DiscordClientFactory
{

    /// <summary>
    /// Constructs the adapter or runtime object from supplied context and dependencies.
    /// </summary>
    /// <param name="settings">The settings supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    public static DiscordClient Create(BotSettings settings)
    {
        var token = settings.Token ?? throw new Exception("DISCORD_TOKEN is not set.");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentNullException(nameof(settings));
        }

        return DiscordClientBuilder.CreateDefault(token, DiscordIntents.All)
            .ConfigureLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Warning);
        })
            .Build();
    }
}
