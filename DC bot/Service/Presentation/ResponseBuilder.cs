using DC_bot.Helper;
using DSharpPlus.Entities;
using DC_bot.Constants;
using DC_bot.Configuration;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Presentation;

/// <summary>
/// Builds localized usage, success, warning, validation, and error responses.
/// </summary>
/// <param name="localization">The localization service dependency used by this boundary.</param>
/// <param name="botSettings">Provides the configured text-command prefix for localized usage examples.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
public class ResponseBuilder(
    ILocalizationService localization,
    ILogger<ResponseBuilder>? logger = null,
    IOptions<BotSettings>? botSettings = null) : IResponseBuilder
{
    private const int MaxEmbedTitleLength = 256;
    private const int MaxEmbedDescriptionLength = 4096;
    private const int MaxEmbedFooterLength = 2048;
    private const int MaxEmbedTotalLength = 6000;
    private const int MaxDiscordMessageLength = 2000;
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

        if (errorKey.EndsWith("_command_usage", StringComparison.Ordinal))
        {
            await SendUsageAsync(message, errorKey[..^"_command_usage".Length], cancellationToken);
            return;
        }

        await SafeRespondAsync(message, BuildEmbed(message, LocalizationKeys.ResponseErrorTitle,
            GetForMessage(message, errorKey), DiscordColor.Red, LocalizationKeys.ResponseValidationFooter),
            "SendValidationErrorAsync", cancellationToken);

        if (errorKey is LocalizationKeys.VolumeCommandInvalid
            or LocalizationKeys.SeekCommandInvalid
            or LocalizationKeys.FiltersCommandInvalid
            or LocalizationKeys.BassBoostCommandInvalid
            or LocalizationKeys.FiltersParameterInvalid
            or LocalizationKeys.QueueCommandInvalid
            or LocalizationKeys.DiscoveryInvalid
            or LocalizationKeys.FiltersAdvancedInvalid
            or LocalizationKeys.FilterPresetInvalid)
        {
            var token = (message.Content ?? string.Empty).Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();
            var prefix = botSettings?.Value.Prefix ?? "!";

            if (token is not null)
            {
                var name = token.StartsWith(prefix, StringComparison.Ordinal) ? token[prefix.Length..] : token;
                await SendUsageAsync(message, name, cancellationToken);
            }
        }
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
        await SafeRespondAsync(message, BuildEmbed(message, LocalizationKeys.ResponseUsageTitle,
            GetForMessage(message, $"{commandName}_command_usage", botSettings?.Value.Prefix ?? "!"),
            DiscordColor.Azure, LocalizationKeys.ResponseUsageFooter), "SendUsageAsync", cancellationToken);
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
        await SafeRespondAsync(message, BuildEmbed(message, LocalizationKeys.ResponseSuccessTitle,
            GetForMessage(message, localizationKey, args), DiscordColor.Green, LocalizationKeys.ResponseSuccessFooter),
            "SendSuccessAsync", cancellationToken);
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
        await SafeRespondAsync(message, BuildEmbed(message, LocalizationKeys.ResponseWarningTitle,
            text, DiscordColor.Gold, LocalizationKeys.ResponseWarningFooter), "SendWarningAsync", cancellationToken);
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
        await SafeRespondAsync(message, BuildEmbed(message, LocalizationKeys.ResponseErrorTitle,
            text, DiscordColor.Red, LocalizationKeys.ResponseErrorFooter), "SendErrorAsync", cancellationToken);
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

    /// <summary>Builds a localized status embed within individual and aggregate Discord text limits.</summary>
    private DiscordEmbed BuildEmbed(
        IDiscordMessage message,
        string titleKey,
        string description,
        DiscordColor color,
        string footerKey)
    {
        return CreateEmbed(message.Channel.Guild?.Id, description, color, titleKey, footerKey);
    }

    /// <inheritdoc />
    public DiscordEmbed CreateEmbed(
        ulong? guildId,
        string description,
        DiscordColor? color = null,
        string? titleKey = null,
        string? footerKey = null)
    {
        var title = DiscordTextSanitizer.Truncate(
            GetForGuild(guildId, titleKey ?? LocalizationKeys.ResponseUsageTitle), MaxEmbedTitleLength);
        var body = DiscordTextSanitizer.Truncate(description, MaxEmbedDescriptionLength);
        var footerLimit = Math.Min(MaxEmbedFooterLength, MaxEmbedTotalLength - title.Length - body.Length);
        var footer = footerKey is null ? null : DiscordTextSanitizer.Truncate(
            GetForGuild(guildId, footerKey, botSettings?.Value.Prefix ?? "!"), footerLimit);
        return new DiscordEmbedBuilder
        {
            Title = title,
            Description = body,
            Color = color ?? DiscordColor.Azure,
            Footer = footer is null ? null : new DiscordEmbedBuilder.EmbedFooter { Text = footer }
        }.Build();
    }

    /// <summary>Resolves presentation labels using the selected guild or default language.</summary>
    private string GetForGuild(ulong? guildId, string key, params object[] args)
    {
        return guildId.HasValue ? localization.Get(guildId.Value, key, args) : localization.Get(key, args);
    }

    /// <summary>Attempts embed delivery and falls back to bounded plain text after a non-cancellation failure.</summary>
    private async Task SafeRespondAsync(
        IDiscordMessage message,
        DiscordEmbed embed,
        string operation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await message.RespondAsync(embed).WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.ResponseSendFailed(exception, operation);
            var fallback = string.IsNullOrWhiteSpace(embed.Description) ? embed.Title ?? string.Empty : embed.Description;

            if (string.IsNullOrWhiteSpace(fallback))
            {
                return;
            }

            fallback = operation switch
            {
                "SendWarningAsync" => FormatWithPrefix(message, WarningPrefixKey, fallback),
                "SendErrorAsync" or "SendValidationErrorAsync" => FormatWithPrefix(message, ErrorPrefixKey, fallback),
                _ => fallback
            };
            await SafeRespondAsync(message, DiscordTextSanitizer.Truncate(fallback, MaxDiscordMessageLength),
                operation + ".Fallback", cancellationToken);
        }
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
