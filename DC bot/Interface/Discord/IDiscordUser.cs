namespace DC_bot.Interface.Discord;

/// <summary>
/// Defines the discord user boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IDiscordUser
{

    /// <summary>
    /// The Discord identifier of this object.
    /// </summary>
    ulong Id { get; }

    /// <summary>
    /// Whether the Discord account belongs to a bot.
    /// </summary>
    bool IsBot { get; }

    /// <summary>
    /// The Discord username.
    /// </summary>
    string Username { get; }

    /// <summary>
    /// The Discord mention text for this user.
    /// </summary>
    string Mention { get; }
}
