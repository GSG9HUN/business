using DSharpPlus.Entities;

namespace DC_bot.Wrapper.SlashCommands;

/// <summary>
/// Defines the slash command context adapter boundary so callers are independent of its concrete implementation.
/// </summary>
internal interface ISlashCommandContextAdapter
{

    /// <summary>Gets the guild associated with the slash-command context.</summary>
    DiscordGuild? Guild { get; }

    /// <summary>Gets the response channel associated with the slash-command context.</summary>
    DiscordChannel Channel { get; }

    /// <summary>Gets the invoking user associated with the slash-command context.</summary>
    DiscordUser User { get; }

    /// <summary>Gets the guild member associated with the slash-command context.</summary>
    DiscordMember? Member { get; }

    /// <summary>
    /// Defers a slash response using the requested visibility policy.
    /// </summary>
    /// <returns>A task representing completion of this operation.</returns>
    Task DeferResponseAsync();

    /// <summary>
    /// Sends a response through the wrapped message or interaction boundary.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task RespondAsync(string message);

    /// <summary>
    /// Sends a response through the wrapped message or interaction boundary.
    /// </summary>
    /// <param name="embed">The embed supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task RespondAsync(DiscordEmbed embed);

    /// <summary>
    /// Updates the previously acknowledged interaction response.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task EditResponseAsync(string message);

    /// <summary>
    /// Updates the previously acknowledged interaction response.
    /// </summary>
    /// <param name="embed">The embed supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task EditResponseAsync(DiscordEmbed embed);

    /// <summary>
    /// Sends a follow-up message after interaction acknowledgement.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task FollowupAsync(string message);

    /// <summary>
    /// Sends a follow-up message after interaction acknowledgement.
    /// </summary>
    /// <param name="embed">The embed supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task FollowupAsync(DiscordEmbed embed);
}
