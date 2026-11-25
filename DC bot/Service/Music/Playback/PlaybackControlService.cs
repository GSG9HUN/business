using System.Globalization;
using DC_bot.BotControl;
using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Events;
using DC_bot.Interface.Service.Music.Lifecycle;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Music.Presentation;
using DC_bot.Interface.Service.Music.Queue;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using DC_bot.Service.Music.Controls;
using DC_bot.Service.Music.Lifecycle.Models;
using DC_bot.Service.Music.Lifecycle.PersistentVoice;
using DC_bot.Service.Music.Lifecycle.Sessions;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Music.Playback;

/// <summary>
/// Inspects playback and applies audio, position, skip, previous, pause, resume, and leave controls under session ownership.
/// </summary>
/// <param name="musicQueueService">The music queue service dependency used by this boundary.</param>
/// <param name="trackPlaybackService">The track playback service dependency used by this boundary.</param>
/// <param name="sessions">The sessions supplied for this operation.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="trackNotificationService">The track notification service dependency used by this boundary.</param>
/// <param name="playerConnectionService">The player connection service dependency used by this boundary.</param>
/// <param name="playbackStateRepository">The playback state repository dependency used by this boundary.</param>
/// <param name="queueRepository">The queue repository dependency used by this boundary.</param>
/// <param name="persistentVoiceLeaveService">Disables persistent voice before owned cleanup and batch closure.</param>
/// <param name="lifecycleOptions">The configured typed options consumed by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="musicControls">Applies guild voting and DJ skip policy.</param>
/// <param name="trackEndedHandlerService">The track ended handler service dependency used by this boundary.</param>
public class PlaybackControlService(
    IMusicQueueService musicQueueService,
    ITrackPlaybackService trackPlaybackService,
    GuildPlaybackSessions sessions,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ITrackNotificationService trackNotificationService,
    IPlayerConnectionService playerConnectionService,
    IPlaybackStateRepository playbackStateRepository,
    IQueueRepository queueRepository,
    PersistentVoiceLeaveService persistentVoiceLeaveService,
    IOptions<PlaybackLifecycleOptions> lifecycleOptions,
    ILogger<PlaybackControlService> logger,
    ITrackEndedHandlerService trackEndedHandlerService,
    MusicControlSettingsService musicControls) : IPlaybackControlService
{

    private const int MaxVolumePercent = 200;

    /// <inheritdoc />
    public async Task<PlaybackControlResult> NowPlayingAsync(
        IDiscordMessage message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var guildId = message.Channel.Guild.Id;
        var session = sessions.CaptureCurrent(guildId);

        if (session is null || session.Player.CurrentTrack is not { } track)
        {
            return RejectAudioCommand(guildId, LocalizationKeys.NowPlayingCommandNoTrack);
        }

        try
        {
            var queueItemId = sessions.GetReadyQueueItemId(session);
            var state = await playbackStateRepository.GetOrCreateAsync(guildId, cancellationToken);
            var item = queueItemId.HasValue
                ? await queueRepository.GetByIdAsync(queueItemId.Value, cancellationToken)
                : null;

            if (!sessions.IsCurrent(session)
                || sessions.GetReadyQueueItemId(session) != queueItemId
                || !ReferenceEquals(session.Player.CurrentTrack, track))
            {
                return RejectAudioCommand(guildId, ValidationErrorKeys.PlaybackSuperseded);
            }

            var unknown = localizationService.Get(guildId, LocalizationKeys.PlaybackDetailsUnknown);
            var repeat = state.IsRepeating ? "track" : state.IsRepeatingList ? "list" : "off";
            var pause = session.Player.State == PlayerState.Paused ? "paused" : "playing";
            var effects = sessions.HasPendingFilterChange(session)
                ? localizationService.Get(guildId, LocalizationKeys.FiltersCommandPending)
                : GetConfiguredEffects(session.Player.Filters);
            var response = localizationService.Get(guildId, LocalizationKeys.NowPlayingDesignBody,
                DC_bot.Helper.DiscordTextSanitizer.EscapeMentions(track.Title),
                DC_bot.Helper.DiscordTextSanitizer.EscapeMentions(track.Author),
                ((char)96) + FormatPlaybackTimestamp(GetCurrentPosition(session.Player)) + " / "
                    + (track.IsLiveStream ? localizationService.Get(guildId, LocalizationKeys.PlaybackDetailsLive)
                        : FormatPlaybackTimestamp(track.Duration)) + ((char)96),
                (session.Player.Volume * 100f).ToString("0.##", CultureInfo.InvariantCulture));
            return PlaybackControlResult.Information(response);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not inspect current playback. Guild={GuildId}", guildId);
            return RejectAudioCommand(guildId, ValidationErrorKeys.LavalinkError);
        }
    }

    /// <inheritdoc />
    public Task<PlaybackControlResult> VolumeAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string? value,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var guildId = message.Channel.Guild.Id;

        if (string.IsNullOrWhiteSpace(value))
        {
            var session = sessions.CaptureCurrent(guildId);
            return Task.FromResult(session is null
                ? RejectAudioCommand(guildId, ValidationErrorKeys.BotIsNotConnectedError)
                : PlaybackControlResult.Information(localizationService.Get(guildId, LocalizationKeys.VolumeCommandCurrent,
                    (session.Player.Volume * 100f).ToString("0.##", CultureInfo.InvariantCulture))));
        }

        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var percent)
            || percent < 0
            || percent > MaxVolumePercent)
        {
            return Task.FromResult(RejectAudioCommand(guildId, LocalizationKeys.VolumeCommandInvalid));
        }

        return ExecuteAudioCommandAsync(message, member, "Volume", async (session, reservation) =>
        {
            if (session.Player is not LavalinkPlayer player)
            {
                return RejectAudioCommand(guildId, LocalizationKeys.PlaybackAudioUnsupported);
            }

            await sessions.RunTransportAsync(session, async () =>
            {
                sessions.ValidateControl(session, reservation);
                await player.SetVolumeAsync(percent / 100f, cancellationToken);
                return true;
            });
            sessions.ValidateControl(session, reservation);
            sessions.RememberConfirmedVolume(session, percent / 100f);
            var amplification = percent > 100
                ? localizationService.Get(guildId, LocalizationKeys.VolumeCommandAmplified)
                : string.Empty;
            return PlaybackControlResult.Succeeded(localizationService.Get(
                guildId, LocalizationKeys.VolumeCommandResponse, percent, amplification));
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PlaybackControlResult> SeekAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string position,
        CancellationToken cancellationToken = default)
    {
        return SeekTrackAsync(message, member, position, "seek", 0, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PlaybackControlResult> ForwardAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string seconds,
        CancellationToken cancellationToken = default)
    {
        return SeekTrackAsync(message, member, seconds, "forward", 1, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PlaybackControlResult> RewindAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string seconds,
        CancellationToken cancellationToken = default)
    {
        return SeekTrackAsync(message, member, seconds, "rewind", -1, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PlaybackControlResult> RestartAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default)
    {
        return SeekTrackAsync(message, member, "0", "restart", 0, cancellationToken);
    }

    /// <summary>Validates an invariant seek argument and updates position under existing playback ownership.</summary>
    private Task<PlaybackControlResult> SeekTrackAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string argument,
        string operation,
        int direction,
        CancellationToken cancellationToken)
    {
        var guildId = message.Channel.Guild.Id;

        if (!TryParsePlaybackTime(argument, direction != 0, out var offset))
        {
            return Task.FromResult(RejectAudioCommand(guildId, LocalizationKeys.SeekCommandInvalid));
        }

        return ExecuteAudioCommandAsync(message, member, operation, async (session, reservation) =>
        {
            if (sessions.HasPendingFilterChange(session))
            {
                return RejectAudioCommand(guildId, LocalizationKeys.FiltersCommandOutcomeUnknown);
            }

            var player = session.Player;
            var track = player.CurrentTrack;

            if (track is null)
            {
                return RejectAudioCommand(guildId, LocalizationKeys.NowPlayingCommandNoTrack);
            }

            if (!track.IsSeekable
                || track.IsLiveStream
                || track.Duration <= TimeSpan.Zero)
            {
                return RejectAudioCommand(guildId, LocalizationKeys.SeekCommandUnsupported);
            }

            var queueItemId = sessions.GetReadyQueueItemId(session);

            if (!queueItemId.HasValue)
            {
                return RejectAudioCommand(guildId, ValidationErrorKeys.PlaybackBusy);
            }

            var requested = direction == 0 ? offset
                : GetCurrentPosition(player) + TimeSpan.FromTicks(offset.Ticks * direction);
            var target = requested;

            if (direction == 0 && target >= track.Duration)
            {
                return RejectAudioCommand(guildId, LocalizationKeys.SeekCommandOutsideTrack);
            }

            if (direction != 0)
            {
                target = TimeSpan.FromTicks(Math.Clamp(requested.Ticks, 0,
                    Math.Max(0, track.Duration.Ticks - TimeSpan.TicksPerMillisecond)));
            }

            var paused = player.State == PlayerState.Paused;
            await sessions.RunTransportAsync(session, async () =>
            {
                ValidateSeekTarget(session, reservation, queueItemId.Value);
                await player.SeekAsync(target, cancellationToken);
                ValidateSeekTarget(session, reservation, queueItemId.Value);
                return true;
            });
            await sessions.MutateAsync(session, async () =>
            {
                ValidateSeekTarget(session, reservation, queueItemId.Value);
                await playbackStateRepository.SetPlaybackPositionAsync(
                    guildId, target, paused, MobileRealtimeEventNames.PlaybackPositionChanged, cancellationToken);
                return true;
            });
            var clamped = target != requested
                ? localizationService.Get(guildId, LocalizationKeys.SeekCommandClamped)
                : string.Empty;
            return PlaybackControlResult.Succeeded(localizationService.Get(guildId, $"{operation}_command_response",
                $"{track.Author} - {track.Title}", FormatPlaybackTimestamp(target), FormatPlaybackTimestamp(track.Duration), clamped))
                with { TrackDisplayName = $"{track.Author} - {track.Title}" };
        }, cancellationToken);
    }

    /// <summary>Checks current player and queue occurrence ownership before applying or persisting a seek.</summary>
    private void ValidateSeekTarget(PlaybackSession session, ControlReservation reservation, long queueItemId)
    {
        sessions.ValidateControl(session, reservation);

        if (sessions.GetReadyQueueItemId(session) != queueItemId)
        {
            throw new PlaybackSessionExpiredException();
        }
    }

    /// <summary>Runs an audio command in the user's existing voice session under an exclusive reservation.</summary>
    private async Task<PlaybackControlResult> ExecuteAudioCommandAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string operation,
        Func<PlaybackSession, ControlReservation, Task<PlaybackControlResult>> action,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var guildId = message.Channel.Guild.Id;
        var (player, channel, _, valid) = await playerConnectionService.TryGetAndValidateExistingPlayerAsync(
            message, member?.VoiceState?.Channel, cancellationToken);

        if (!valid || player is null || channel is null)
        {
            return RejectAudioCommand(guildId, ValidationErrorKeys.BotIsNotConnectedError);
        }

        if (channel.Guild.Id != guildId
            || player.VoiceChannelId != channel.Id)
        {
            return RejectAudioCommand(guildId, LocalizationKeys.PlaybackWrongVoiceChannel);
        }

        var session = sessions.Capture(guildId, player);

        if (session is null)
        {
            return RejectAudioCommand(guildId, ValidationErrorKeys.PlaybackSuperseded);
        }

        ControlReservation? reservation = null;
        try
        {
            reservation = sessions.ReserveControl(session, operation);
            return await action(session, reservation);
        }
        catch (PlaybackSessionExpiredException)
        {
            return RejectAudioCommand(guildId, ValidationErrorKeys.PlaybackSuperseded);
        }
        catch (PlaybackOperationBusyException)
        {
            return RejectAudioCommand(guildId, ValidationErrorKeys.PlaybackBusy);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Audio command outcome uncertain. Guild={GuildId} Operation={Operation}", guildId, operation);
            return RejectAudioCommand(guildId, LocalizationKeys.PlaybackAudioOutcomeUnknown);
        }
        finally
        {
            if (reservation is not null)
            {
                sessions.ReleaseControl(reservation);
            }
        }
    }

    /// <summary>Builds an expected localized audio-command rejection without an external side effect.</summary>
    private PlaybackControlResult RejectAudioCommand(ulong guildId, string key)
    {
        return PlaybackControlResult.Failed(localizationService.Get(guildId, key), key);
    }

    /// <summary>Parses nonnegative integer seconds or a strict colon-delimited absolute timestamp without overflow.</summary>
    private static bool TryParsePlaybackTime(string value, bool secondsOnly, out TimeSpan result)
    {
        result = TimeSpan.Zero;
        var parts = value.Split(':');

        if (parts.Length is < 1 or > 3 || (secondsOnly && parts.Length != 1))
        {
            return false;
        }

        long total = 0;

        for (var index = 0; index < parts.Length; index++)
        {
            var part = parts[index];

            if (!long.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out var number)
                || (index > 0 && number >= 60)
                || number > int.MaxValue)
            {
                return false;
            }

            total = total * 60 + number;

            if (total > int.MaxValue)
            {
                return false;
            }
        }

        result = TimeSpan.FromSeconds(total);
        return true;
    }

    /// <summary>Formats source-track time with hours when needed and without wrapping elapsed minutes.</summary>
    private static string FormatPlaybackTimestamp(TimeSpan value)
    {
        return value.TotalHours >= 1
            ? $"{(long)value.TotalHours:D2}:{value.Minutes:D2}:{value.Seconds:D2}"
            : $"{(long)value.TotalMinutes:D2}:{value.Seconds:D2}";
    }

    /// <summary>Lists configured SDK filter names without modifying their values.</summary>
    internal static string GetConfiguredEffects(IPlayerFilters filters)
    {
        var configured = new (string Name, object? Options)[]
        {
            ("channel-mix", filters.ChannelMix),
            ("distortion", filters.Distortion),
            ("equalizer", filters.Equalizer),
            ("karaoke", filters.Karaoke),
            ("low-pass", filters.LowPass),
            ("rotation", filters.Rotation),
            ("timescale", filters.Timescale),
            ("tremolo", filters.Tremolo),
            ("vibrato", filters.Vibrato),
            ("filter-volume", filters.Volume)
        };
        return string.Join(", ", configured.Where(filter => filter.Options is not null)
            .Select(filter => filter.Name));
    }


    /// <summary>
    /// Pauses the validated player and persists playback position and pause state.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<PlaybackControlResult> PauseAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await ExecuteSessionBoundControlAsync(message, member, operationName: "Pause", transportOperation: player => player.PauseAsync(cancellationToken: cancellationToken), persistenceOperation: (guildId, player) => playbackStateRepository.SetPlaybackPositionAsync(guildId, GetCurrentPosition(player), true, MobileRealtimeEventNames.PlaybackPaused), noTrackLocalizationKey: LocalizationKeys.PauseCommandError, successLocalizationKey: LocalizationKeys.PauseCommandResponse, noTrackErrorCode: LocalizationKeys.PauseCommandError, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Resumes the validated player and persists updated playback state.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<PlaybackControlResult> ResumeAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await ExecuteSessionBoundControlAsync(message, member, operationName: "Resume", transportOperation: player => player.ResumeAsync(cancellationToken: cancellationToken), persistenceOperation: (guildId, player) => playbackStateRepository.SetPlaybackPositionAsync(guildId, GetCurrentPosition(player), false, MobileRealtimeEventNames.PlaybackResumed), noTrackLocalizationKey: LocalizationKeys.ResumeCommandError, successLocalizationKey: LocalizationKeys.ResumeCommandResponse, noTrackErrorCode: LocalizationKeys.ResumeCommandError, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Stops and completes the current attempt, then advances without relying on a later SDK Stopped event.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<PlaybackControlResult> SkipAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default)
    {
        return SkipCoreAsync(message, member, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PlaybackControlResult> SkipVotedAttemptAsync(
        IDiscordMessage message,
        IDiscordMember member,
        Guid attempt,
        CancellationToken cancellationToken = default)
    {
        return SkipCoreAsync(message, member, attempt, cancellationToken);
    }

    /// <summary>Applies a manual skip or a maintenance vote bound to its original attempt.</summary>
    private async Task<PlaybackControlResult> SkipCoreAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        Guid? expectedAttempt,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var(connection, channel, guildId, isValid) = await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel, cancellationToken: cancellationToken);

        if (!isValid || connection is null || channel is null)
        {
            return PlaybackControlResult.Failed("Could not resolve an active Lavalink player.", ValidationErrorKeys.BotIsNotConnectedError);
        }

        var session = sessions.Capture(guildId, connection);

        if (session is null)
        {
            return PlaybackControlResult.Failed("Playback session expired.", ValidationErrorKeys.PlaybackSuperseded);
        }

        ControlReservation? reservation = null;
        try
        {
            reservation = sessions.ReserveControl(session, "Skip");
            var currentTrack = connection.CurrentTrack;

            if (expectedAttempt.HasValue
                && sessions.GetReadyAttemptId(session) != expectedAttempt)
            {
                return RejectAudioCommand(guildId, ValidationErrorKeys.PlaybackSuperseded);
            }

            if (channel.Guild.Id != guildId
                || connection.VoiceChannelId != member?.VoiceState?.Channel?.Id)
            {
                return RejectAudioCommand(guildId, LocalizationKeys.PlaybackWrongVoiceChannel);
            }

            if (currentTrack is not null)
            {
                var vote = await musicControls.EvaluateSkipAsync(session, message, member, cancellationToken, expectedAttempt.HasValue);
                sessions.ValidateControl(session, reservation);

                if (vote is not null)
                {
                    return vote;
                }
            }

            if (currentTrack is null
                && !await musicQueueService.HasTracks(guildId, cancellationToken))
            {
                await trackNotificationService.SendValidationAsync(message.Channel, LocalizationKeys.SkipCommandError, cancellationToken);
                return PlaybackControlResult.Failed(localizationService.Get(guildId, LocalizationKeys.SkipCommandError), LocalizationKeys.SkipCommandError);
            }

            if (currentTrack is null)
            {
                var next = await trackPlaybackService.TryPlayNextTrackAsync(session, message.Channel, reservation: reservation, cancellationToken: cancellationToken);

                if (
                    next.Status is
                    not (PlaybackStartStatus.Started or PlaybackStartStatus.StartedStatePending or PlaybackStartStatus.Unknown))
                {
                    return PlaybackControlResult.Failed("Next track was not started.", ValidationErrorKeys.LavalinkError);
                }
            }
            else
            {
                await sessions.RunTransportAsync(session, async () =>
                {
                    await connection.StopAsync(cancellationToken: cancellationToken);
                    return true;
                });
                sessions.ValidateControl(session, reservation);
                var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                // StopAsync's successful response is our stop confirmation. The SDK's
                // separate Stopped event is ignored, so it cannot advance twice.
                var accepted = sessions.OnTrackEnded(session, currentTrack.ToString(), async (operationId, queueItemId) =>
                {
                    try
                    {
                        await trackEndedHandlerService.HandleTrackEndedAsync(session, operationId, queueItemId, new TrackEndedEventArgs(connection, currentTrack, TrackEndReason.Stopped), message.Channel);
                        completed.TrySetResult();
                    }
                    catch (Exception ex)
                    {
                        completed.TrySetException(ex);
                    }
                });
                // The end handler uses the same control reservation mechanism.
                sessions.ReleaseControl(reservation);
                reservation = null;

                if (accepted)
                {
                    await completed.Task;
                }
            }

            if (!sessions.IsCurrent(session))
            {
                throw new PlaybackSessionExpiredException();
            }

            logger.LogInformation("Skip requested for guild {GuildId}. Current playback stopped.", guildId);
            var started = session.Player.CurrentTrack;
            var response = started is null
                ? localizationService.Get(guildId, LocalizationKeys.SkipCommandResponse)
                : localizationService.Get(guildId, LocalizationKeys.SkipDesignNext,
                    DC_bot.Helper.DiscordTextSanitizer.EscapeMentions(started.Title), FormatPlaybackTimestamp(started.Duration));
            return PlaybackControlResult.Succeeded(response);
        }
        catch (PlaybackSessionExpiredException)
        {
            return PlaybackControlResult.Failed("Playback session expired.", ValidationErrorKeys.PlaybackSuperseded);
        }
        catch (PlaybackOperationBusyException)
        {
            return PlaybackControlResult.Failed("Another playback operation is active.", ValidationErrorKeys.PlaybackBusy);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "SkipAsync");
            return PlaybackControlResult.Failed("Lavalink operation failed.", ValidationErrorKeys.LavalinkError);
        }
        finally
        {
            if (reservation is not null)
            {
                sessions.ReleaseControl(reservation);
            }
        }
    }

    /// <summary>
    /// Starts the preceding canonical item in the active batch, reporting when no earlier item is available.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<PlaybackControlResult> PreviousAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var(connection, channel, guildId, valid) = await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel, cancellationToken: cancellationToken);

        if (!valid || connection is null || channel is null)
        {
            return PlaybackControlResult.Failed("Could not resolve an active Lavalink player.", ValidationErrorKeys.BotIsNotConnectedError);
        }

        var session = sessions.Capture(guildId, connection);

        if (session is null)
        {
            return PlaybackControlResult.Failed("Playback session expired.", ValidationErrorKeys.PlaybackSuperseded);
        }

        ControlReservation? reservation = null;
        try
        {
            reservation = sessions.ReserveControl(session, "Previous");
            var previous = await queueRepository.GetPreviousItemAsync(guildId, cancellationToken: cancellationToken);
            sessions.ValidateControl(session, reservation);

            if (previous is null)
            {
                return PlaybackControlResult.Failed(localizationService.Get(guildId, LocalizationKeys.PreviousCommandError), LocalizationKeys.PreviousCommandError);
            }

            var result = await trackPlaybackService.StartExistingAsync(session, message.Channel, previous.Id, PlaybackStartReason.Previous, reservation: reservation, cancellationToken: cancellationToken);
            return result.Status switch
            {
                PlaybackStartStatus.Started => PlaybackControlResult.Succeeded("Previous track started."),
                PlaybackStartStatus.StartedStatePending => new PlaybackControlResult(true, "Playback started; state reconciliation is pending.", ValidationErrorKeys.PlaybackStatePending),
                PlaybackStartStatus.Unknown => new PlaybackControlResult(true, "Playback outcome is being reconciled.", ValidationErrorKeys.PlaybackOutcomeUnknown),
                PlaybackStartStatus.Superseded => PlaybackControlResult.Failed("Previous track was superseded.", ValidationErrorKeys.PlaybackSuperseded),
                PlaybackStartStatus.Busy => PlaybackControlResult.Failed("Another playback operation is active.", ValidationErrorKeys.PlaybackBusy),
                PlaybackStartStatus.Empty => PlaybackControlResult.Failed(localizationService.Get(guildId, LocalizationKeys.PreviousCommandError), LocalizationKeys.PreviousCommandError),
                _ => PlaybackControlResult.Failed("Previous track was not started.", ValidationErrorKeys.LavalinkError)};
        }
        catch (PlaybackSessionExpiredException)
        {
            return PlaybackControlResult.Failed("Playback session expired.", ValidationErrorKeys.PlaybackSuperseded);
        }
        catch (PlaybackOperationBusyException)
        {
            return PlaybackControlResult.Failed("Another playback operation is active.", ValidationErrorKeys.PlaybackBusy);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "PreviousAsync");
            return PlaybackControlResult.Failed("Lavalink operation failed.", ValidationErrorKeys.LavalinkError);
        }
        finally
        {
            if (reservation is not null)
            {
                sessions.ReleaseControl(reservation);
            }
        }
    }

    /// <summary>
    /// Runs shared leave cleanup and closes the active batch after all applicable cleanup phases succeed.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task LeaveVoiceChannel(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var channel = member?.VoiceState?.Channel;

        if (channel is null)
        {
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.UserNotInVoiceChannel, cancellationToken);
            return;
        }

        var guildId = channel.Guild.Id;
        var leaving = persistentVoiceLeaveService.LeaveAsync(guildId);
        try
        {
            var result = await leaving.WaitAsync(lifecycleOptions.Value.LeaveDeadline);

            if (!result.FullySucceeded)
            {
                await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError, cancellationToken);
            }
        }
        catch (TimeoutException ex)
        {
            logger.LogWarning(ex, "Leave cleanup is pending. Guild={GuildId}", guildId);
            _ = ObserveLeaveAsync(leaving);
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Leave cleanup failed. Guild={GuildId}", guildId);
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError, cancellationToken);
        }

        async Task ObserveLeaveAsync(Task<LeaveResult> task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Late leave cleanup failed. Guild={GuildId}", guildId);
            }
        }
    }

    /// <summary>
    /// Reads the player&apos;s current position for a playback-state update.
    /// </summary>
    /// <param name="player">The player supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    private static TimeSpan GetCurrentPosition(Lavalink4NET.Players.ILavalinkPlayer player)
    {
        var position = player.Position?.Position ?? TimeSpan.Zero;

        if (position < TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }

        var duration = player.CurrentTrack?.Duration ?? TimeSpan.Zero;
        return duration > TimeSpan.Zero && position > duration ? duration : position;
    }

    /// <summary>
    /// Applies a player control under a validated session and exclusive reservation.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="operationName">The operation name supplied for this operation.</param>
    /// <param name="transportOperation">The transport operation supplied for this operation.</param>
    /// <param name="persistenceOperation">The persistence operation supplied for this operation.</param>
    /// <param name="noTrackLocalizationKey">The no track localization key supplied for this operation.</param>
    /// <param name="successLocalizationKey">The success localization key supplied for this operation.</param>
    /// <param name="noTrackErrorCode">The no track error code supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<PlaybackControlResult> ExecuteSessionBoundControlAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string operationName,
        Func<ILavalinkPlayer, ValueTask> transportOperation,
        Func<ulong, ILavalinkPlayer, Task>? persistenceOperation,
        string noTrackLocalizationKey,
        string successLocalizationKey,
        string noTrackErrorCode,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var(connection, channel, guildId, isValid) = await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel, cancellationToken: cancellationToken);

        if (!isValid || connection is null || channel is null)
        {
            return PlaybackControlResult.Failed("Could not resolve an active Lavalink player.", ValidationErrorKeys.BotIsNotConnectedError);
        }

        var session = sessions.Capture(guildId, connection);

        if (session is null)
        {
            return PlaybackControlResult.Failed("Playback session expired.", ValidationErrorKeys.PlaybackSuperseded);
        }

        ControlReservation? reservation = null;
        try
        {
            reservation = sessions.ReserveControl(session, operationName);
            // Check the track after acquiring the shared reservation.

            var currentTrack = connection.CurrentTrack;

            if (currentTrack is null)
            {
                await trackNotificationService.SendValidationAsync(channel, noTrackLocalizationKey, cancellationToken);
                return PlaybackControlResult.Failed(localizationService.Get(guildId, noTrackLocalizationKey), noTrackErrorCode);
            }

            var trackDisplayName = $"{currentTrack.Author} - {currentTrack.Title}";
            await sessions.RunTransportAsync(session, async () =>
            {
                await transportOperation(connection);
                return true;
            });

            if (persistenceOperation is not null)
            {
                await sessions.MutateAsync(session, async () =>
                {
                    await persistenceOperation(guildId, connection);
                    return true;
                });
            }

            sessions.ValidateControl(session, reservation);
            return PlaybackControlResult.Succeeded(localizationService.Get(guildId, successLocalizationKey, trackDisplayName))
                with { TrackDisplayName = trackDisplayName };
        }
        catch (PlaybackSessionExpiredException)
        {
            return PlaybackControlResult.Failed("Playback session expired.", ValidationErrorKeys.PlaybackSuperseded);
        }
        catch (PlaybackOperationBusyException)
        {
            return PlaybackControlResult.Failed("Another playback operation is active.", ValidationErrorKeys.PlaybackBusy);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, operationName);
            return PlaybackControlResult.Failed("Lavalink operation failed.", ValidationErrorKeys.LavalinkError);
        }
        finally
        {
            if (reservation is not null)
            {
                sessions.ReleaseControl(reservation);
            }
        }
    }

    /// <inheritdoc />
    public async Task<PlaybackControlResult> QueueCommandAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string operation,
        string arguments,
        CancellationToken cancellationToken = default)
    {
        var guildId = message.Channel.Guild.Id;

        if (operation == "queuefind")
        {
            if (string.IsNullOrWhiteSpace(arguments) || arguments.Length > 200)
            {
                return RejectAudioCommand(guildId, LocalizationKeys.QueueCommandInvalid);
            }

            var found = await musicQueueService.FindAsync(guildId, arguments.Trim(), cancellationToken);
            var lines = string.Join("\n", found.Select(item => $"{item.Key}. {item.Value}"));
            return PlaybackControlResult.Succeeded(localizationService.Get(guildId,
                found.Count == 0 ? LocalizationKeys.QueueFindCommandEmpty : LocalizationKeys.QueueFindCommandResult, lines));
        }

        var parts = arguments.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var required = operation == "move" ? 2 : 1;

        if (operation is not ("remove" or "move" or "skipto")
            || parts.Length != required
            || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var from)
            || from < 1)
        {
            return RejectAudioCommand(guildId, LocalizationKeys.QueueCommandInvalid);
        }

        var to = 0;

        if (operation == "move"
            && (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out to) || to < 1))
        {
            return RejectAudioCommand(guildId, LocalizationKeys.QueueCommandInvalid);
        }

        return await ExecuteAudioCommandAsync(message, member, operation, async (session, reservation) =>
        {
            if (operation == "remove")
            {
                var removed = await sessions.MutateAsync(session,
                    () => musicQueueService.RemoveAt(guildId, from, cancellationToken));
                sessions.ValidateControl(session, reservation);
                return removed.Success
                    ? PlaybackControlResult.Succeeded(localizationService.Get(guildId, LocalizationKeys.RemoveCommandResult,
                        from, removed.RemovedTrackTitle ?? "?", removed.QueueSize))
                    : RejectAudioCommand(guildId, LocalizationKeys.QueueCommandChanged);
            }

            if (operation == "move")
            {
                var moved = await sessions.MutateAsync(session,
                    () => musicQueueService.MoveToIndex(guildId, from - 1, to - 1, cancellationToken));
                sessions.ValidateControl(session, reservation);
                if (!moved.Success)
                {
                    return RejectAudioCommand(guildId, LocalizationKeys.QueueCommandChanged);
                }

                var response = localizationService.Get(guildId, LocalizationKeys.MoveCommandResult, from, to);
                try
                {
                    var preview = await musicQueueService.FindAsync(guildId, string.Empty, cancellationToken);
                    sessions.ValidateControl(session, reservation);
                    response += localizationService.Get(guildId, LocalizationKeys.QueueCommandPreview,
                        string.Join("\n", preview.Select(item => $"{item.Key}. {item.Value}")));
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception, "Queue moved but order preview unavailable. Guild={GuildId}", guildId);
                }

                return PlaybackControlResult.Succeeded(response);
            }

            if (!await musicControls.CanSkipDirectlyAsync(session, message, member, cancellationToken))
            {
                return RejectAudioCommand(guildId, LocalizationKeys.MusicVoteRequired);
            }

            sessions.ValidateControl(session, reservation);
            var snapshot = await queueRepository.GetQueuedItemsAsync(guildId, cancellationToken);
            sessions.ValidateControl(session, reservation);
            var waiting = snapshot.Where(item => item.State == DC_bot.Interface.Service.Persistence.Models.Queue.QueueItemState.Queued)
                .ToArray();

            if (from > waiting.Length)
            {
                return RejectAudioCommand(guildId, LocalizationKeys.QueueCommandChanged);
            }

            var ids = waiting.Select(item => item.Id)
                .ToArray();
            var started = await trackPlaybackService.StartQueueSelectionAsync(session, message.Channel,
                waiting[from - 1].Id, ids, reservation, cancellationToken);
            return started.Status switch
            {
                PlaybackStartStatus.Started => PlaybackControlResult.Succeeded(
                    localizationService.Get(guildId, LocalizationKeys.SkipToCommandResult, from)),
                PlaybackStartStatus.Busy or PlaybackStartStatus.Empty => RejectAudioCommand(guildId, LocalizationKeys.QueueCommandChanged),
                PlaybackStartStatus.Superseded => RejectAudioCommand(guildId, ValidationErrorKeys.PlaybackSuperseded),
                PlaybackStartStatus.StartedStatePending => RejectAudioCommand(guildId, ValidationErrorKeys.PlaybackStatePending),
                PlaybackStartStatus.Unknown => RejectAudioCommand(guildId, ValidationErrorKeys.PlaybackOutcomeUnknown),
                _ => RejectAudioCommand(guildId, ValidationErrorKeys.LavalinkError)
            };
        }, cancellationToken);
    }

}
