using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service.Presentation;

/// <summary>
/// Builds localized usage, success, warning, validation, and error responses.
/// </summary>
/// <param name="localization">The localization service dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
public class ResponseBuilder(ILocalizationService localization, ILogger<ResponseBuilder>? logger = null) : IResponseBuilder
{
    private const string ErrorPrefixKey = "response_error_prefix";
    private const string WarningPrefixKey = "response_warning_prefix";
    private readonly ILogger<ResponseBuilder> _logger = logger ?? NullLogger<ResponseBuilder>.Instance;

    /// <summary>
    /// Sends a localized user-validation error for the command message.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="errorKey">The error key supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task SendValidationErrorAsync(
        IDiscordMessage message,
        string errorKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrEmpty(errorKey))
        {
            return;
        }

        await SafeRespondAsync(message, GetForMessage(message, errorKey), "SendValidationErrorAsync", cancellationToken);
    }

    /// <summary>
    /// Sends localized usage guidance for the requested command.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="commandName">The command name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task SendUsageAsync(IDiscordMessage message, string commandName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await SafeRespondAsync(message, GetForMessage(message, $"{commandName}_command_usage"), "SendUsageAsync", cancellationToken);
    }

    /// <summary>
    /// Sends a localized success response with formatting arguments.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="localizationKey">The localization key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task SendSuccessAsync(
        IDiscordMessage message,
        string localizationKey,
        CancellationToken cancellationToken,
        params object[] args)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await SafeRespondAsync(message, GetForMessage(message, localizationKey, args), "SendSuccessAsync", cancellationToken);
    }

    /// <summary>
    /// Sends a localized success response with formatting arguments.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="localizationKey">The localization key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task SendSuccessAsync(IDiscordMessage message, string localizationKey, params object[] args) => SendSuccessAsync(message, localizationKey, CancellationToken.None, args);

    /// <summary>
    /// Sends a localized warning response with formatting arguments.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="localizationKey">The localization key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task SendWarningAsync(
        IDiscordMessage message,
        string localizationKey,
        CancellationToken cancellationToken,
        params object[] args)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var text = GetForMessage(message, localizationKey, args);
        await SafeRespondAsync(message, FormatWithPrefix(message, WarningPrefixKey, text), "SendWarningAsync", cancellationToken);
    }

    /// <summary>
    /// Sends a localized warning response with formatting arguments.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="localizationKey">The localization key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task SendWarningAsync(IDiscordMessage message, string localizationKey, params object[] args) => SendWarningAsync(message, localizationKey, CancellationToken.None, args);

    /// <summary>
    /// Sends a localized error response with formatting arguments.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="localizationKey">The localization key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task SendErrorAsync(
        IDiscordMessage message,
        string localizationKey,
        CancellationToken cancellationToken,
        params object[] args)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var text = GetForMessage(message, localizationKey, args);
        await SafeRespondAsync(message, FormatWithPrefix(message, ErrorPrefixKey, text), "SendErrorAsync", cancellationToken);
    }

    /// <summary>
    /// Sends a localized error response with formatting arguments.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="localizationKey">The localization key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task SendErrorAsync(IDiscordMessage message, string localizationKey, params object[] args) => SendErrorAsync(message, localizationKey, CancellationToken.None, args);

    /// <summary>
    /// Builds logging context from the message&apos;s available guild, channel, user, and message identities.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="key">The key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private string GetForMessage(IDiscordMessage message, string key, params object[] args)
    {
        var guild = message.Channel.Guild;
        return guild is null ? localization.Get(key, args) : localization.Get(guild.Id, key, args);
    }

    /// <summary>
    /// Adds a localized severity prefix to a formatted response.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="prefixKey">The prefix key supplied for this operation.</param>
    /// <param name="text">The text supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private string FormatWithPrefix(IDiscordMessage message, string prefixKey, string text)
    {
        var prefix = GetForMessage(message, prefixKey);
        return prefix == prefixKey ? text : $"{prefix}{text}";
    }

    /// <summary>
    /// Attempts a command response while containing Discord delivery failures.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="text">The text supplied for this operation.</param>
    /// <param name="operation">The operation supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private async Task SafeRespondAsync(
        IDiscordMessage message,
        string text,
        string operation,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await message.RespondAsync(text)
                .WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.ResponseSendFailed(ex, operation);
        }
    }
}
