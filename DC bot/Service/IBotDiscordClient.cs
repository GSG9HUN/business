namespace DC_bot.Service;

/// <summary>
/// Defines the bot discord client boundary so callers are independent of its concrete implementation.
/// </summary>
internal interface IBotDiscordClient
{

    /// <summary>
    /// Connects the configured Discord or Lavalink runtime boundary and propagates connection failures.
    /// </summary>
    /// <returns>A task representing completion of this operation.</returns>
    Task ConnectAsync();
}
