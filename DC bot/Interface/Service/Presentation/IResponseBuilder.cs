using DSharpPlus.Entities;
using DC_bot.Interface.Discord;

namespace DC_bot.Interface.Service.Presentation;

/// <summary>
/// Defines the response builder boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IResponseBuilder
{

    /// <summary>Creates a bounded, guild-localized embed for channel notifications and interaction responses.</summary>
    /// <param name="guildId">The guild language scope, or null for the default catalog.</param>
    /// <param name="description">The formatted response text.</param>
    /// <param name="color">The status color; omitted values use Azure.</param>
    /// <param name="titleKey">The localized title key; omitted values use the informational title.</param>
    /// <param name="footerKey">The optional localized footer key.</param>
    /// <returns>An embed within Discord text limits.</returns>
    DiscordEmbed CreateEmbed(
        ulong? guildId,
        string description,
        DiscordColor? color = null,
        string? titleKey = null,
        string? footerKey = null);


    /// <summary>
    /// Sends a localized user-validation error for the command message.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="errorKey">The error key supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task SendValidationErrorAsync(IDiscordMessage message, string errorKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends localized usage guidance for the requested command.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="commandName">The command name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task SendUsageAsync(IDiscordMessage message, string commandName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a localized success response with formatting arguments.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="localizationKey">The localization key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task SendSuccessAsync(IDiscordMessage message, string localizationKey, CancellationToken cancellationToken, params object[] args);

    /// <summary>
    /// Sends a localized success response with formatting arguments.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="localizationKey">The localization key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task SendSuccessAsync(IDiscordMessage message, string localizationKey, params object[] args);

    /// <summary>
    /// Sends a localized warning response with formatting arguments.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="localizationKey">The localization key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task SendWarningAsync(IDiscordMessage message, string localizationKey, CancellationToken cancellationToken, params object[] args);

    /// <summary>
    /// Sends a localized warning response with formatting arguments.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="localizationKey">The localization key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task SendWarningAsync(IDiscordMessage message, string localizationKey, params object[] args);

    /// <summary>
    /// Sends a localized error response with formatting arguments.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="localizationKey">The localization key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task SendErrorAsync(IDiscordMessage message, string localizationKey, CancellationToken cancellationToken, params object[] args);

    /// <summary>
    /// Sends a localized error response with formatting arguments.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="localizationKey">The localization key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task SendErrorAsync(IDiscordMessage message, string localizationKey, params object[] args);
}
