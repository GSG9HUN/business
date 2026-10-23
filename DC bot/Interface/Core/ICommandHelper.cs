using DC_bot.Helper.Validation;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Presentation;
using Microsoft.Extensions.Logging;

namespace DC_bot.Interface.Core;

/// <summary>
/// Defines the command helper boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ICommandHelper
{

    /// <summary>
    /// Runs user validation and sends a localized failure response when validation fails.
    /// </summary>
    /// <param name="userValidation">The user validation service dependency used by this boundary.</param>
    /// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<UserValidationResult?> TryValidateUserAsync(
        IUserValidationService userValidation,
        IResponseBuilder responseBuilder,
        IDiscordMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a required argument and presents usage guidance when absent.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="commandName">The command name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<string?> TryGetArgumentAsync(
        IDiscordMessage message,
        IResponseBuilder responseBuilder,
        ILogger logger,
        string commandName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses a playlist name and source argument while preserving quoted values.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="commandName">The command name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<(string, string)?> TryParseSavePlaylistArguments(
        IDiscordMessage message,
        IResponseBuilder responseBuilder,
        ILogger logger,
        string commandName,
        CancellationToken cancellationToken = default);
}
