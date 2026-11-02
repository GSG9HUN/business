using DC_bot.Configuration;

namespace DC_bot.Startup;

/// <summary>
/// Groups bot, Lavalink, and database settings needed by application startup.
/// </summary>
/// <param name="BotSettings">The bot settings supplied for this operation.</param>
/// <param name="LavalinkSettings">The lavalink settings supplied for this operation.</param>
/// <param name="PostgresConnectionString">The postgres connection string supplied for this operation.</param>
internal sealed record BotRuntimeSettings(
    BotSettings BotSettings,
    LavalinkSettings LavalinkSettings,
    string PostgresConnectionString);
