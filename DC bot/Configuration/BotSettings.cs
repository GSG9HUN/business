namespace DC_bot.Configuration;

/// <summary>
/// Defines Discord authentication and the text-command prefix.
/// </summary>
public sealed class BotSettings
{

    /// <summary>
    /// The Discord bot authentication token.
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    /// The prefix used to recognize text commands.
    /// </summary>
    public string Prefix { get; init; } = "!";
}
