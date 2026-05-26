using DC_bot.Configuration;

namespace DC_bot.Startup;

internal sealed record BotRuntimeSettings(
    BotSettings BotSettings,
    LavalinkSettings LavalinkSettings,
    string PostgresConnectionString);
