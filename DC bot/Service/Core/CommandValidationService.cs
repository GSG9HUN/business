using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Presentation;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Core;

/// <summary>
/// Parses command arguments and presents user-validation failures before execution.
/// </summary>
public class CommandValidationService : ICommandHelper
{

    /// <summary>
    /// Runs user validation and sends a localized failure response when validation fails.
    /// </summary>
    /// <param name="userValidation">The user validation service dependency used by this boundary.</param>
    /// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<UserValidationResult?> TryValidateUserAsync(
        IUserValidationService userValidation,
        IResponseBuilder responseBuilder,
        IDiscordMessage message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validationResult = await userValidation.ValidateUserAsync(message, cancellationToken);

        if (validationResult.IsValid is false)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey, cancellationToken: cancellationToken);
            return null;
        }

        return validationResult;
    }

    /// <summary>
    /// Resolves a required argument and presents usage guidance when absent.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="commandName">The command name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<string?> TryGetArgumentAsync(
        IDiscordMessage message,
        IResponseBuilder responseBuilder,
        ILogger logger,
        string commandName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var args = message.Content.Split(" ", 2);

        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            await responseBuilder.SendUsageAsync(message, commandName, cancellationToken);
            logger.LogInformation("The user not provided arguments for {CommandName}", commandName);
            return null;
        }

        return args[1].Trim();
    }

    /// <summary>
    /// Parses a playlist name and source argument while preserving quoted values.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="commandName">The command name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<(string, string)?> TryParseSavePlaylistArguments(
        IDiscordMessage message,
        IResponseBuilder responseBuilder,
        ILogger logger,
        string commandName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!TryReadCommandPayload(message.Content, out var payload)
            || !TryReadArgument(payload, out var firstArgument, out var remainingPayload)
            || string.IsNullOrWhiteSpace(remainingPayload))
        {
            await responseBuilder.SendUsageAsync(message, commandName, cancellationToken);
            logger.LogInformation("The user has not provided arguments for {CommandName}", commandName);
            return null;
        }

        var secondArgument = ReadRemainingArgument(remainingPayload);

        if (string.IsNullOrWhiteSpace(firstArgument)
            || string.IsNullOrWhiteSpace(secondArgument))
        {
            await responseBuilder.SendUsageAsync(message, commandName, cancellationToken);
            logger.LogInformation("The user has not provided arguments for {CommandName}", commandName);
            return null;
        }

        return (firstArgument.Trim(), secondArgument.Trim());
    }

    /// <summary>
    /// Deserializes a persisted mobile-command payload and reports invalid input.
    /// </summary>
    /// <param name="content">The content supplied for this operation.</param>
    /// <param name="payload">The payload supplied for this operation.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    private static bool TryReadCommandPayload(string content, out string payload)
    {
        payload = string.Empty;
        var parts = content.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            return false;
        }

        payload = parts[1].Trim();
        return payload.Length > 0;
    }

    /// <summary>
    /// Reads the next quoted or unquoted argument and advances the parsing offset.
    /// </summary>
    /// <param name="payload">The payload supplied for this operation.</param>
    /// <param name="argument">The argument supplied for this operation.</param>
    /// <param name="remainingPayload">The remaining payload supplied for this operation.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    private static bool TryReadArgument(string payload, out string argument, out string remainingPayload)
    {
        argument = string.Empty;
        remainingPayload = string.Empty;
        payload = payload.TrimStart();

        if (payload.Length == 0)
        {
            return false;
        }

        if (payload[0] != '"')
        {
            var nextWhitespace = payload.IndexOfAny([' ', '\t']);

            if (nextWhitespace < 0)
            {
                argument = payload;
                return true;
            }

            argument = payload[..nextWhitespace];
            remainingPayload = payload[nextWhitespace..].TrimStart();
            return true;
        }

        var value = new List<char>();
        var escaped = false;

        for (var index = 1; index < payload.Length; index++)
        {
            var current = payload[index];

            if (escaped)
            {
                value.Add(current);
                escaped = false;
                continue;
            }

            if (current == '\\')
            {
                escaped = true;
                continue;
            }

            if (current == '"')
            {
                argument = new string (value.ToArray());
                remainingPayload = payload[(index + 1)..].TrimStart();
                return true;
            }

            value.Add(current);
        }

        return false;
    }

    /// <summary>
    /// Returns unconsumed argument text after trimming surrounding whitespace.
    /// </summary>
    /// <param name="payload">The payload supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private static string ReadRemainingArgument(string payload)
    {
        payload = payload.Trim();

        if (payload.Length == 0)
        {
            return string.Empty;
        }

        if (payload[0] != '"')
        {
            return payload;
        }

        return TryReadArgument(payload, out var argument, out var remainingPayload) && string.IsNullOrWhiteSpace(remainingPayload) ? argument : payload;
    }
}
