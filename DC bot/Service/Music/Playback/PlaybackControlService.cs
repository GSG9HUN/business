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
using DC_bot.Service.Music.Lifecycle;
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
/// Applies skip, previous, pause, resume, and leave under exclusive playback ownership.
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
    ITrackEndedHandlerService trackEndedHandlerService) : IPlaybackControlService
{

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
    public async Task<PlaybackControlResult> SkipAsync(
        IDiscordMessage message,
        IDiscordMember? member,
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
            reservation = sessions.ReserveControl(session, "Skip");
            var currentTrack = connection.CurrentTrack;

            if (currentTrack is null
                && !await musicQueueService.HasTracks(guildId, cancellationToken))
            {
                await trackNotificationService.SendSafeAsync(message.Channel, localizationService.Get(guildId, LocalizationKeys.SkipCommandError), "SkipAsync.NoTrack", cancellationToken);
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
            return PlaybackControlResult.Succeeded(localizationService.Get(guildId, LocalizationKeys.SkipCommandResponse));
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
            if (connection.CurrentTrack is null)
            {
                await trackNotificationService.SendSafeAsync(channel, localizationService.Get(guildId, noTrackLocalizationKey), $"{operationName}.NoTrack", cancellationToken);
                return PlaybackControlResult.Failed(localizationService.Get(guildId, noTrackLocalizationKey), noTrackErrorCode);
            }

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
            return PlaybackControlResult.Succeeded(localizationService.Get(guildId, successLocalizationKey));
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
}
