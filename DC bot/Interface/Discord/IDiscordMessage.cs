using DSharpPlus.Entities;

namespace DC_bot.Interface.Discord;

/// <summary>
/// Defines the discord message boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IDiscordMessage
{

    /// <summary>
    /// The Discord identifier of this object.
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// The message text.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// The Discord channel associated with this object.
    /// </summary>
    public IDiscordChannel Channel { get; set; }

    /// <summary>
    /// The author of the message.
    /// </summary>
    public IDiscordUser Author { get; set; }

    /// <summary>
    /// The timestamp at which the Discord message was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The embeds attached to the message.
    /// </summary>
    public IReadOnlyList<DiscordEmbed> Embeds { get; set; }

    /// <summary>
    /// Sends a response through the wrapped message or interaction boundary.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task RespondAsync(string message);

    /// <summary>
    /// Sends a response through the wrapped message or interaction boundary.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task RespondAsync(DiscordEmbed message);

    /// <summary>
    /// Updates the wrapped Discord message or interaction response.
    /// </summary>
    /// <param name="builder">The builder supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task ModifyAsync(DiscordMessageBuilder builder);
}
