using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Interface.Service.SlashCommands;

/// <summary>
/// Defines the slash interaction context boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ISlashInteractionContext
{

    /// <summary>
    /// The Discord guild identifier, or null outside a guild.
    /// </summary>
    ulong? GuildId { get; }

    /// <summary>
    /// The guild associated with this object.
    /// </summary>
    IDiscordGuild? Guild { get; }

    /// <summary>
    /// The Discord channel associated with this object.
    /// </summary>
    IDiscordChannel Channel { get; }

    /// <summary>
    /// The user associated with this object.
    /// </summary>
    IDiscordUser User { get; }

    /// <summary>
    /// The Discord guild member associated with this operation, or null when unavailable.
    /// </summary>
    IDiscordMember? Member { get; }

    /// <summary>
    /// Whether the interaction has been acknowledged with a deferred response.
    /// </summary>
    bool IsDeferred { get; }

    /// <summary>
    /// Whether an initial interaction response has been sent.
    /// </summary>
    bool HasResponded { get; }

    /// <summary>
    /// Acknowledges an interaction so its response can be completed after command execution.
    /// </summary>
    /// <returns>A task representing completion of this operation.</returns>
    Task DeferAsync();

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
    /// Creates a command-message adapter that sends responses through this slash interaction.
    /// </summary>
    /// <param name="commandName">The command name supplied for this operation.</param>
    /// <param name="argument">The argument supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    IDiscordMessage CreateMessage(string commandName, string? argument = null);
}
