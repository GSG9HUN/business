using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Interface.Service.Presentation.Models;
using DC_bot.Helper;
using DSharpPlus.Entities;
using DC_bot.Constants;
using DC_bot.Configuration;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using System.Globalization;
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
/// <param name="commandRegistry">Resolves registered command casing for usage catalog lookups.</param>
public class ResponseBuilder(
    ILocalizationService localization,
    ILogger<ResponseBuilder>? logger = null,
    IOptions<BotSettings>? botSettings = null,
    ICommandRegistry? commandRegistry = null) : IResponseBuilder
{
    /// <summary>The approved success accent.</summary>
    public static readonly DiscordColor SuccessColor = new(0x38db59);

    /// <summary>The approved warning accent.</summary>
    public static readonly DiscordColor WarningColor = new(0xf6c728);

    /// <summary>The approved error accent.</summary>
    public static readonly DiscordColor ErrorColor = new(0xed4245);

    /// <summary>The approved information accent.</summary>
    public static readonly DiscordColor InfoColor = new(0x3498db);

    private const int MaxEmbedTitleLength = 256;
    private const int MaxEmbedDescriptionLength = 4096;
    private const int MaxEmbedFooterLength = 2048;
    private const int MaxEmbedTotalLength = 6000;
    private const int MaxDiscordMessageLength = 2000;
    private const string ErrorPrefixKey = LocalizationKeys.ResponseErrorPrefix;
    private const string WarningPrefixKey = LocalizationKeys.ResponseWarningPrefix;
    private static readonly IReadOnlyDictionary<string, string> CommandTitleKeys =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [CommandNames.NowPlaying] = LocalizationKeys.ResponseNowPlayingTitle,
            [CommandNames.Volume] = LocalizationKeys.ResponseVolumeTitle,
            [CommandNames.Filters] = LocalizationKeys.ResponseFiltersTitle,
            [CommandNames.BassBoost] = LocalizationKeys.ResponseFiltersTitle,
            [CommandNames.Speed] = LocalizationKeys.ResponseFiltersTitle,
            [CommandNames.Pitch] = LocalizationKeys.ResponseFiltersTitle,
            [CommandNames.Karaoke] = LocalizationKeys.ResponseFiltersTitle,
            [CommandNames.MusicStatus] = LocalizationKeys.ResponseMusicStatusTitle,
            [CommandNames.MusicConfig] = LocalizationKeys.ResponseMusicSettingsTitle,
            [CommandNames.SearchPicker] = LocalizationKeys.ResponseSearchPickerTitle,
            [CommandNames.Autoplay] = LocalizationKeys.ResponseAutoplayTitle,
            [CommandNames.Lyrics] = LocalizationKeys.ResponseLyricsTitle,
            [CommandNames.Favorite] = LocalizationKeys.ResponseFavoritesTitle,
            [CommandNames.Recommendations] = LocalizationKeys.ResponseRecommendationsTitle,
            [CommandNames.Remove] = LocalizationKeys.ResponseQueueTitle,
            [CommandNames.Move] = LocalizationKeys.ResponseQueueTitle,
            [CommandNames.SkipTo] = LocalizationKeys.ResponseQueueTitle,
            [CommandNames.QueueFind] = LocalizationKeys.ResponseQueueSearchTitle,
            [CommandNames.Skip] = LocalizationKeys.ResponseSkipTitle,
            [CommandNames.Seek] = LocalizationKeys.ResponseSeekTitle,
            [CommandNames.Forward] = LocalizationKeys.ResponseSeekTitle,
            [CommandNames.Rewind] = LocalizationKeys.ResponseSeekTitle,
            [CommandNames.Restart] = LocalizationKeys.ResponseSeekTitle,
        };
    private static readonly IReadOnlyDictionary<string, (string Success, string Information)> ResultTitleKeys =
        new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase)
        {
            [CommandNames.Volume] = (LocalizationKeys.ResponseVolumeSuccessTitle, LocalizationKeys.ResponseVolumeInfoTitle),
            [CommandNames.Speed] = (LocalizationKeys.ResponseSpeedSuccessTitle, LocalizationKeys.ResponseSpeedInfoTitle),
            [CommandNames.Pitch] = (LocalizationKeys.ResponsePitchSuccessTitle, LocalizationKeys.ResponsePitchInfoTitle),
            [CommandNames.BassBoost] = (LocalizationKeys.ResponseBassBoostSuccessTitle, LocalizationKeys.ResponseBassBoostInfoTitle),
            [CommandNames.Filters] = (LocalizationKeys.ResponseFiltersSuccessTitle, LocalizationKeys.ResponseFiltersInfoTitle),
            [CommandNames.Karaoke] = (LocalizationKeys.ResponseKaraokeSuccessTitle, LocalizationKeys.ResponseKaraokeInfoTitle),
            [CommandNames.Remove] = (LocalizationKeys.ResponseRemoveSuccessTitle, LocalizationKeys.ResponseRemoveInfoTitle),
            [CommandNames.Move] = (LocalizationKeys.ResponseMoveSuccessTitle, LocalizationKeys.ResponseMoveInfoTitle),
            [CommandNames.SkipTo] = (LocalizationKeys.ResponseSkipToSuccessTitle, LocalizationKeys.ResponseSkipToInfoTitle),
            [CommandNames.Skip] = (LocalizationKeys.ResponseSkipSuccessTitle, LocalizationKeys.ResponseSkipInfoTitle),
            [CommandNames.Seek] = (LocalizationKeys.ResponseSeekSuccessTitle, LocalizationKeys.ResponseSeekInfoTitle),
            [CommandNames.Forward] = (LocalizationKeys.ResponseForwardSuccessTitle, LocalizationKeys.ResponseForwardInfoTitle),
            [CommandNames.Rewind] = (LocalizationKeys.ResponseRewindSuccessTitle, LocalizationKeys.ResponseRewindInfoTitle),
            [CommandNames.Restart] = (LocalizationKeys.ResponseRestartSuccessTitle, LocalizationKeys.ResponseRestartInfoTitle),
            [CommandNames.Autoplay] = (LocalizationKeys.ResponseAutoplaySuccessTitle, LocalizationKeys.ResponseAutoplayInfoTitle),
            [CommandNames.SearchPicker] = (LocalizationKeys.ResponseSearchPickerSuccessTitle, LocalizationKeys.ResponseSearchPickerInfoTitle),
            [CommandNames.Favorite] = (LocalizationKeys.ResponseFavoriteSuccessTitle, LocalizationKeys.ResponseFavoriteInfoTitle),
        };
    private static readonly IReadOnlyDictionary<string, string> OutcomeTitleKeys =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [LocalizationKeys.CreatePlaylistCommandUnknownError] = LocalizationKeys.ResponseHtmlCreatePlaylistCommandUnknownErrorTitle,
            [LocalizationKeys.ImportPlaylistCommandNoTracksFound] = LocalizationKeys.ResponseHtmlImportPlaylistCommandNoTracksFoundTitle,
            [LocalizationKeys.ImportPlaylistCommandUnknownError] = LocalizationKeys.ResponseHtmlImportPlaylistCommandUnknownErrorTitle,
            [LocalizationKeys.DeletePlaylistCommandDoesNotExist] = LocalizationKeys.ResponseHtmlDeletePlaylistCommandDoesNotExistTitle,
            [LocalizationKeys.DeletePlaylistCommandUnknownError] = LocalizationKeys.ResponseHtmlDeletePlaylistCommandUnknownErrorTitle,
            [LocalizationKeys.RenamePlaylistCommandPlaylistAlreadyExists] = LocalizationKeys.ResponseHtmlRenamePlaylistCommandPlaylistAlreadyExistsTitle,
            [LocalizationKeys.RenamePlaylistCommandUnknownError] = LocalizationKeys.ResponseHtmlRenamePlaylistCommandUnknownErrorTitle,
            [LocalizationKeys.LoadPlaylistCommandEmptyPlaylist] = LocalizationKeys.ResponseHtmlLoadPlaylistCommandEmptyPlaylistTitle,
            [LocalizationKeys.LoadPlaylistCommandUnknownError] = LocalizationKeys.ResponseHtmlLoadPlaylistCommandUnknownErrorTitle,
            [LocalizationKeys.ShuffleCommandNotEnoughTracks] = LocalizationKeys.ResponseHtmlShuffleCommandNotEnoughTracksTitle,
            [LocalizationKeys.LanguageCommandError] = LocalizationKeys.ResponseHtmlLanguageCommandErrorTitle,
            [LocalizationKeys.HistoryCommandEmpty] = LocalizationKeys.ResponseHtmlHistoryCommandEmptyTitle,
            [LocalizationKeys.PingCommandResponse] = LocalizationKeys.ResponseOutcomePingCommandResponseTitle,
            [LocalizationKeys.RepeatCommandRepeatingOn] = LocalizationKeys.ResponseOutcomeRepeatCommandRepeatingOnTitle,
            [LocalizationKeys.RepeatCommandRepeatingOff] = LocalizationKeys.ResponseOutcomeRepeatCommandRepeatingOffTitle,
            [LocalizationKeys.RepeatListCommandRepeatingOn] = LocalizationKeys.ResponseOutcomeRepeatListCommandRepeatingOnTitle,
            [LocalizationKeys.RepeatListCommandRepeatingOff] = LocalizationKeys.ResponseOutcomeRepeatListCommandRepeatingOffTitle,
            [LocalizationKeys.AddSongToPlaylistCommandAdded] = LocalizationKeys.ResponseOutcomeAddSongToPlaylistCommandAddedTitle,
            [LocalizationKeys.RemoveSongFromPlaylistCommandRemoved] = LocalizationKeys.ResponseOutcomeRemoveSongFromPlaylistCommandRemovedTitle,
            [LocalizationKeys.CreatePlaylistCommandAlreadyExists] = LocalizationKeys.ResponseOutcomeCreatePlaylistCommandAlreadyExistsTitle,
            [LocalizationKeys.ImportPlaylistCommandAlreadyExists] = LocalizationKeys.ResponseOutcomeImportPlaylistCommandAlreadyExistsTitle,
            [LocalizationKeys.LyricsUnavailable] = LocalizationKeys.ResponseOutcomeLyricsUnavailableTitle,
            [LocalizationKeys.RecommendationsUnavailable] = LocalizationKeys.ResponseOutcomeRecommendationsUnavailableTitle,
            [LocalizationKeys.PreviousCommandError] = LocalizationKeys.ResponseOutcomePreviousCommandErrorTitle,
            [LocalizationKeys.ResumeCommandError] = LocalizationKeys.ResponseOutcomeResumeCommandErrorTitle,
            [LocalizationKeys.SeekCommandInvalid] = LocalizationKeys.ResponseOutcomeSeekCommandInvalidTitle,
            [LocalizationKeys.QueueCommandInvalid] = LocalizationKeys.ResponseOutcomeQueueCommandInvalidTitle,
            [LocalizationKeys.FiltersCommandInvalid] = LocalizationKeys.ResponseOutcomeFiltersCommandInvalidTitle,
            [LocalizationKeys.FiltersCommandOperationFailed] = LocalizationKeys.ResponseOutcomeFiltersCommandOperationFailedTitle,
            [LocalizationKeys.FilterPresetMissing] = LocalizationKeys.ResponseOutcomeFilterPresetMissingTitle,
            [LocalizationKeys.FavoriteMissing] = LocalizationKeys.ResponseOutcomeFavoriteMissingTitle,
            [LocalizationKeys.LanguageCommandInvalidLanguage] = LocalizationKeys.ResponseOutcomeLanguageCommandInvalidLanguageTitle,
        };
    private readonly ILogger<ResponseBuilder> _logger = logger ?? NullLogger<ResponseBuilder>.Instance;

    /// <inheritdoc />
    public PaginationRequest BuildPlaylistPages(ulong guildId, ViewPlaylistResult result)
    {
        var title = localization.Get(guildId, LocalizationKeys.PlaylistDesignTitle,
            DiscordTextSanitizer.EscapeMentions(result.PlaylistName.Trim()));
        var rows = result.Tracks.Select(track => localization.Get(guildId,
                LocalizationKeys.ViewPlaylistCommandTrack, track.OrderNumber,
                DiscordTextSanitizer.EscapeMentions(track.Title)))
            .ToArray();
        return DiscordPaginationService.FromItems(title, rows);
    }

    /// <inheritdoc />
    public PaginationRequest BuildQueuePages(
        ulong guildId,
        IReadOnlyCollection<ILavaLinkTrack> tracks,
        ILavaLinkTrack? current)
    {
        var rows = tracks.Select((track, index) => localization.Get(guildId, LocalizationKeys.QueueDesignRow,
                index + 1, DiscordTextSanitizer.EscapeMentions(track.Title), FormatTrackDuration(track.Duration)))
            .ToArray();
        var request = DiscordPaginationService.FromItems(localization.Get(guildId, LocalizationKeys.QueueCommandEmbedTitle), rows);
        var heading = current is null ? string.Empty
            : localization.Get(guildId, LocalizationKeys.QueueDesignCurrent, DiscordTextSanitizer.EscapeMentions(current.Title));

        return new PaginationRequest(async (number, token) =>
        {
            var page = await request.LoadPageAsync(number, token);
            return page with { Description = heading + page.Description };
        });
    }

    /// <summary>Formats provider durations without wrapping hours or total minutes.</summary>
    private static string FormatTrackDuration(TimeSpan duration) =>
        duration.TotalHours >= 1 ? duration.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
            : duration.ToString(@"m\:ss", CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public Task SendHistoryAsync(
        IDiscordMessage message,
        IReadOnlyCollection<PlaybackBatchRecord> batches,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var guildId = message.Channel.Guild.Id;
        var rows = batches.Select(batch =>
        {
            var status = localization.Get(guildId, batch.CompletedAtUtc is null
                ? LocalizationKeys.HistoryCommandActive : LocalizationKeys.HistoryCommandCompleted);
            var entry = localization.Get(guildId, LocalizationKeys.HistoryCommandEntry,
                batch.TrackCount, batch.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture));
            return $"**#{batch.Id} · {status}**\n{entry}";
        });
        return SafeRespondAsync(message, CreateEmbed(guildId, string.Join("\n\n", rows),
            DiscordColor.Azure, LocalizationKeys.HistoryCommandTitle), nameof(SendHistoryAsync), cancellationToken);
    }

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

        await SafeRespondAsync(message, CreateValidationEmbed(message.Channel.Guild?.Id, errorKey),
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

        if (commandRegistry is not null
            && commandRegistry.TryGetCommand(commandName, out var command))
        {
            commandName = command.Name;
        }

        await SafeRespondAsync(message, CreateEmbed(message.Channel.Guild?.Id,
            GetForMessage(message, $"{commandName}_command_usage", botSettings?.Value.Prefix ?? "!"),
            DiscordColor.Azure, LocalizationKeys.ResponseUsageTitle, LocalizationKeys.ResponseUsageFooter), nameof(SendUsageAsync), cancellationToken);
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
        await SafeRespondAsync(message, CreateEmbed(message.Channel.Guild?.Id,
            GetForMessage(message, localizationKey, args), DiscordColor.Green,
            GetResponseTitleKey(message, localizationKey, LocalizationKeys.ResponseSuccessTitle)),
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
        await SafeRespondAsync(message, CreateEmbed(message.Channel.Guild?.Id, text, DiscordColor.Gold,
            GetResponseTitleKey(message, localizationKey, LocalizationKeys.ResponseWarningTitle)), "SendWarningAsync", cancellationToken);
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
        await SafeRespondAsync(message, CreateEmbed(message.Channel.Guild?.Id, text, DiscordColor.Red,
            GetResponseTitleKey(message, localizationKey, LocalizationKeys.ResponseErrorTitle)), "SendErrorAsync", cancellationToken);
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
    /// Resolves guild-localized response text and context-specific guidance.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="key">The key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private string GetForMessage(IDiscordMessage message, string key, params object[] args)
    {
        var guild = message.Channel.Guild;
        var text = guild is null ? localization.Get(key, args) : localization.Get(guild.Id, key, args);

        return text;
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

    /// <inheritdoc />
    public DiscordEmbed CreateValidationEmbed(ulong? guildId, string errorKey)
    {
        var userCondition = errorKey is ValidationErrorKeys.UserNotInVoiceChannel
            or ValidationErrorKeys.BotIsNotConnectedError
            or ValidationErrorKeys.QueueCapacityExceeded
            or ValidationErrorKeys.PlaybackStatePending
            or ValidationErrorKeys.PlaybackOutcomeUnknown
            or ValidationErrorKeys.PlaybackSuperseded
            or ValidationErrorKeys.PlaybackBusy
            or LocalizationKeys.PlaybackWrongVoiceChannel
            or LocalizationKeys.NowPlayingCommandNoTrack
            or LocalizationKeys.PauseCommandError
            or LocalizationKeys.ResumeCommandError
            or LocalizationKeys.SkipCommandError
            or LocalizationKeys.PreviousCommandError
            or LocalizationKeys.VolumeCommandInvalid
            or LocalizationKeys.SeekCommandInvalid
            or LocalizationKeys.SeekCommandUnsupported
            or LocalizationKeys.PlaybackAudioUnsupported
            or LocalizationKeys.PlaybackAudioOutcomeUnknown
            or LocalizationKeys.FiltersCommandInvalid
            or LocalizationKeys.BassBoostCommandInvalid
            or LocalizationKeys.FiltersParameterInvalid
            or LocalizationKeys.FiltersCommandUnsupported
            or LocalizationKeys.FiltersCommandOutcomeUnknown
            or LocalizationKeys.QueueCommandInvalid
            or LocalizationKeys.QueueCommandChanged
            or LocalizationKeys.DiscoveryInvalid
            or LocalizationKeys.DiscoveryNoTrack
            or LocalizationKeys.LyricsUnavailable
            or LocalizationKeys.RecommendationsUnavailable
            or LocalizationKeys.FavoriteEmpty
            or LocalizationKeys.FavoriteMissing
            or LocalizationKeys.FiltersAdvancedInvalid
            or LocalizationKeys.FilterPresetInvalid
            or LocalizationKeys.FilterPresetForbidden
            or LocalizationKeys.FilterPresetMissing
            or LocalizationKeys.FilterPresetExists
            or LocalizationKeys.FilterPresetUnsupported
            or LocalizationKeys.MusicVoteRequired
            or LocalizationKeys.MusicConfigForbidden
            or LocalizationKeys.LanguageCommandInvalidLanguage
            or LocalizationKeys.SlashCommandGuildOnly;
        return CreateEmbed(guildId, GetForGuild(guildId, errorKey),
            userCondition ? DiscordColor.Gold : DiscordColor.Red,
            GetStatusTitleKey(errorKey, userCondition ? LocalizationKeys.ResponseWarningTitle : LocalizationKeys.ResponseErrorTitle));
    }

    /// <inheritdoc />
    public Task SendInfoAsync(
        IDiscordMessage message,
        string localizationKey,
        CancellationToken cancellationToken,
        params object[] args)
    {
        return SafeRespondAsync(message, CreateEmbed(message.Channel.Guild?.Id, GetForMessage(message, localizationKey, args),
            titleKey: GetResponseTitleKey(message, localizationKey, LocalizationKeys.ResponseUsageTitle)),
            "SendInfoAsync", cancellationToken);
    }

    /// <inheritdoc />
    public Task SendInfoAsync(IDiscordMessage message, string localizationKey, params object[] args)
    {
        return SendInfoAsync(message, localizationKey, CancellationToken.None, args);
    }

    /// <inheritdoc />
    public Task SendPlaybackResultAsync(
        IDiscordMessage message,
        PlaybackControlResult result,
        CancellationToken cancellationToken = default)
    {
        if (!result.Success)
        {
            return SendValidationErrorAsync(message, result.ErrorCode ?? ValidationErrorKeys.LavalinkError, cancellationToken);
        }

        if (result.ResponseTitleKey is { } titleKey)
        {
            var color = result.HasWarning ? DiscordColor.Gold
                : result.IsInformational ? DiscordColor.Azure : DiscordColor.Green;
            return SafeRespondAsync(message, CreateEmbed(message.Channel.Guild?.Id, result.Message, color, titleKey),
                nameof(SendPlaybackResultAsync), cancellationToken);
        }

        if (result.HasWarning)
        {
            return SendWarningAsync(message, LocalizationKeys.PlaybackCommandResponse, cancellationToken, result.Message);
        }

        return result.IsInformational
            ? SendInfoAsync(message, LocalizationKeys.PlaybackCommandResponse, cancellationToken, result.Message)
            : SendSuccessAsync(message, LocalizationKeys.PlaybackCommandResponse, cancellationToken, result.Message);
    }

    /// <summary>Resolves a localized result title without changing the command's severity or execution state.</summary>
    private string GetResponseTitleKey(IDiscordMessage message, string localizationKey, string fallbackTitleKey)
    {
        if (localizationKey != LocalizationKeys.PlaybackCommandResponse)
        {
            return GetStatusTitleKey(localizationKey, fallbackTitleKey);
        }

        var command = (message.Content ?? string.Empty).Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? string.Empty;
        var prefix = botSettings?.Value.Prefix ?? "!";

        if (command.StartsWith(prefix, StringComparison.Ordinal))
        {
            command = command[prefix.Length..];
        }

        if (ResultTitleKeys.TryGetValue(command, out var titles))
        {
            if (fallbackTitleKey == LocalizationKeys.ResponseSuccessTitle)
            {
                return titles.Success;
            }

            if (fallbackTitleKey == LocalizationKeys.ResponseUsageTitle)
            {
                return titles.Information;
            }
        }

        return CommandTitleKeys.TryGetValue(command, out var titleKey) ? titleKey : fallbackTitleKey;
    }

    /// <summary>Selects result-specific titles using stable localization keys rather than translated text.</summary>
    private static string GetStatusTitleKey(string key, string fallbackTitleKey)
    {
        if (OutcomeTitleKeys.TryGetValue(key, out var title))
        {
            return title;
        }

        return key switch
        {
            LocalizationKeys.SeekCommandOutsideTrack => LocalizationKeys.ResponseInvalidPositionTitle,
            LocalizationKeys.SeekCommandUnsupported => LocalizationKeys.ResponseSeekUnsupportedTitle,
            LocalizationKeys.VolumeCommandInvalid => LocalizationKeys.ResponseInvalidVolumeTitle,
            LocalizationKeys.FiltersParameterInvalid or LocalizationKeys.FiltersAdvancedInvalid
                or LocalizationKeys.BassBoostCommandInvalid => LocalizationKeys.ResponseInvalidEffectsTitle,
            LocalizationKeys.QueueCommandChanged => LocalizationKeys.ResponseQueueChangedTitle,
            LocalizationKeys.LyricsUnavailable or LocalizationKeys.RecommendationsUnavailable => LocalizationKeys.ResponseProviderTitle,
            LocalizationKeys.MusicConfigForbidden or LocalizationKeys.FilterPresetForbidden => LocalizationKeys.ResponsePermissionTitle,
            LocalizationKeys.HelpCommandUnknown => LocalizationKeys.ResponseUnknownCommandTitle,
            ValidationErrorKeys.UserNotInVoiceChannel => LocalizationKeys.ResponseVoiceRequiredTitle,
            ValidationErrorKeys.BotIsNotConnectedError or ValidationErrorKeys.LavalinkError => LocalizationKeys.ResponsePlaybackUnavailableTitle,
            ValidationErrorKeys.QueueCapacityExceeded => LocalizationKeys.ResponseQueueFullTitle,
            ValidationErrorKeys.PlaybackOutcomeUnknown or ValidationErrorKeys.PlaybackStatePending
                or ValidationErrorKeys.PlaybackBusy => LocalizationKeys.ResponsePlaybackPendingTitle,
            LocalizationKeys.PlaybackWrongVoiceChannel => LocalizationKeys.ResponseSameVoiceTitle,
            LocalizationKeys.NowPlayingCommandNoTrack or LocalizationKeys.SkipCommandError
                or LocalizationKeys.PauseCommandError => LocalizationKeys.ResponseNoTrackTitle,
            LocalizationKeys.ListPlaylistsCommandNoPlaylists => LocalizationKeys.PlaylistDesignNoneTitle,
            LocalizationKeys.ViewPlaylistCommandEmptyPlaylist => LocalizationKeys.PlaylistDesignEmptyTitle,
            LocalizationKeys.FavoriteEmpty => LocalizationKeys.FavoriteDesignEmptyTitle,
            LocalizationKeys.ViewPlaylistCommandPlaylistDoesNotExist => LocalizationKeys.PlaylistDesignMissingTitle,
            LocalizationKeys.ListPlaylistsCommandUnknownError or LocalizationKeys.ViewPlaylistCommandUnknownError
                => LocalizationKeys.PlaylistDesignErrorTitle,
            LocalizationKeys.QueueCommandEmpty => LocalizationKeys.ResponseQueueEmptyTitle,
            LocalizationKeys.TwentyFourSevenCommandEnabled => LocalizationKeys.ResponseAlwaysOnEnabledTitle,
            LocalizationKeys.TwentyFourSevenCommandDisabled => LocalizationKeys.ResponseAlwaysOnDisabledTitle,
            LocalizationKeys.MusicEmptyLeave => LocalizationKeys.ResponsePlaybackClearedTitle,
            LocalizationKeys.ClearCommandResponse => LocalizationKeys.ResponseQueueClearedTitle,
            LocalizationKeys.ShuffleCommandResponse => LocalizationKeys.ResponseShuffleTitle,
            LocalizationKeys.PauseCommandResponse => LocalizationKeys.ResponsePauseTitle,
            LocalizationKeys.ResumeCommandResponse => LocalizationKeys.ResponseResumeTitle,
            LocalizationKeys.PreviousCommandResponse => LocalizationKeys.ResponsePreviousTitle,
            LocalizationKeys.RepeatCommandRepeatingOn or LocalizationKeys.RepeatCommandRepeatingOff
                or LocalizationKeys.RepeatListCommandRepeatingOn or LocalizationKeys.RepeatListCommandRepeatingOff => LocalizationKeys.ResponseRepeatTitle,
            LocalizationKeys.SearchPickerWaiting => LocalizationKeys.ResponseSearchPickerTitle,
            LocalizationKeys.CreatePlaylistCommandCreated => LocalizationKeys.ResponsePlaylistCreatedTitle,
            LocalizationKeys.ImportPlaylistCommandSaved => LocalizationKeys.ResponsePlaylistSavedTitle,
            LocalizationKeys.DeletePlaylistCommandDeleted => LocalizationKeys.ResponsePlaylistDeletedTitle,
            LocalizationKeys.RenamePlaylistCommandRenamed => LocalizationKeys.ResponsePlaylistRenamedTitle,
            LocalizationKeys.AddSongToPlaylistCommandAdded or LocalizationKeys.RemoveSongFromPlaylistCommandRemoved => LocalizationKeys.ResponsePlaylistUpdatedTitle,
            LocalizationKeys.LoadPlaylistCommandLoaded => LocalizationKeys.ResponsePlaylistLoadedTitle,
            LocalizationKeys.LanguageCommandResponse => LocalizationKeys.ResponseLanguageTitle,
            _ => fallbackTitleKey
        };
    }

    /// <inheritdoc />
    public DiscordEmbed CreateEmbed(
        ulong? guildId,
        string description,
        DiscordColor? color = null,
        string? titleKey = null,
        string? footerKey = null)
    {
        var requestedColor = color ?? DiscordColor.Azure;
        var selectedColor = requestedColor.Value == DiscordColor.Green.Value ? SuccessColor
            : requestedColor.Value == DiscordColor.Gold.Value ? WarningColor
            : requestedColor.Value == DiscordColor.Red.Value ? ErrorColor
            : requestedColor.Value == DiscordColor.Azure.Value ? InfoColor : requestedColor;
        var labelKey = selectedColor.Value == SuccessColor.Value ? LocalizationKeys.ResponseSuccessLabel
            : selectedColor.Value == WarningColor.Value ? LocalizationKeys.ResponseWarningLabel
            : selectedColor.Value == ErrorColor.Value ? LocalizationKeys.ResponseErrorLabel : LocalizationKeys.ResponseInfoLabel;
        var label = GetForGuild(guildId, labelKey);
        var title = DiscordTextSanitizer.Truncate(
            GetForGuild(guildId, titleKey ?? LocalizationKeys.ResponseUsageTitle), MaxEmbedTitleLength);
        var body = DiscordTextSanitizer.Truncate(description, MaxEmbedDescriptionLength);
        var footerLimit = Math.Min(MaxEmbedFooterLength, MaxEmbedTotalLength - title.Length - body.Length - label.Length);
        var footer = footerKey is null ? null : DiscordTextSanitizer.Truncate(
            GetForGuild(guildId, footerKey, botSettings?.Value.Prefix ?? "!"), footerLimit);
        return new DiscordEmbedBuilder
        {
            Title = title,
            Description = body,
            Color = selectedColor,
            Footer = footer is null ? null : new DiscordEmbedBuilder.EmbedFooter { Text = footer }
        }.WithAuthor(label)
            .Build();
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
                "SendValidationErrorAsync" when embed.Color?.Value == WarningColor.Value => FormatWithPrefix(message, WarningPrefixKey, fallback),
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
