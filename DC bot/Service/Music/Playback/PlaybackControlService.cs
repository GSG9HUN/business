using DC_bot.BotControl;
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
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using DC_bot.Service.Music.Lifecycle;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Music.Playback;

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
    PlaybackCleanupService playbackCleanupService,
    IOptions<PlaybackLifecycleOptions> lifecycleOptions,
    ILogger<PlaybackControlService> logger,
    ITrackEndedHandlerService trackEndedHandlerService) : IPlaybackControlService
{
    public async Task<PlaybackControlResult> PauseAsync(IDiscordMessage message, IDiscordMember? member)
    {
        return await ExecuteSessionBoundControlAsync(
            message,
            member,
            operationName: "Pause",
            transportOperation: player => player.PauseAsync(),
            persistenceOperation: (guildId, player) => playbackStateRepository.SetPlaybackPositionAsync(
                guildId,
                GetCurrentPosition(player),
                true,
                MobileRealtimeEventNames.PlaybackPaused),
            noTrackLocalizationKey: LocalizationKeys.PauseCommandError,
            successLocalizationKey: LocalizationKeys.PauseCommandResponse,
            noTrackErrorCode: LocalizationKeys.PauseCommandError);
    }

    public async Task<PlaybackControlResult> ResumeAsync(IDiscordMessage message, IDiscordMember? member)
    {
        return await ExecuteSessionBoundControlAsync(
            message,
            member,
            operationName: "Resume",
            transportOperation: player => player.ResumeAsync(),
            persistenceOperation: (guildId, player) => playbackStateRepository.SetPlaybackPositionAsync(
                guildId,
                GetCurrentPosition(player),
                false,
                MobileRealtimeEventNames.PlaybackResumed),
            noTrackLocalizationKey: LocalizationKeys.ResumeCommandError,
            successLocalizationKey: LocalizationKeys.ResumeCommandResponse,
            noTrackErrorCode: LocalizationKeys.ResumeCommandError);
    }

    public async Task<PlaybackControlResult> SkipAsync(
        IDiscordMessage message,
        IDiscordMember? member)
    {
        var (connection, channel, guildId, isValid) =
            await playerConnectionService.TryGetAndValidateExistingPlayerAsync(
                message,
                member?.VoiceState?.Channel);

        if (!isValid || connection is null || channel is null)
        {
            return PlaybackControlResult.Failed(
                "Could not resolve an active Lavalink player.",
                ValidationErrorKeys.BotIsNotConnectedError);
        }

        var session = sessions.Capture(guildId, connection);

        if (session is null)
        {
            return PlaybackControlResult.Failed(
                "Playback session expired.",
                ValidationErrorKeys.PlaybackSuperseded);
        }

        ControlReservation? reservation = null;

        try
        {
            reservation = sessions.ReserveControl(session, "Skip");
            var currentTrack = connection.CurrentTrack;

            if (currentTrack is null &&
                !await musicQueueService.HasTracks(guildId))
            {
                await trackNotificationService.SendSafeAsync(
                    message.Channel,
                    localizationService.Get(
                        guildId,
                        LocalizationKeys.SkipCommandError),
                    "SkipAsync.NoTrack");

                return PlaybackControlResult.Failed(
                    localizationService.Get(
                        guildId,
                        LocalizationKeys.SkipCommandError),
                    LocalizationKeys.SkipCommandError);
            }

            if (currentTrack is null)
            {
                var next = await trackPlaybackService.TryPlayNextTrackAsync(
                    session, message.Channel, reservation: reservation);
                if (next.Status is not (PlaybackStartStatus.Started or
                    PlaybackStartStatus.StartedStatePending or PlaybackStartStatus.Unknown))
                {
                    return PlaybackControlResult.Failed(
                        "Next track was not started.", ValidationErrorKeys.LavalinkError);
                }
            }
            else
            {
                await sessions.RunTransportAsync(session, async () =>
                {
                    await connection.StopAsync();
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
                        await trackEndedHandlerService.HandleTrackEndedAsync(
                            session, operationId, queueItemId,
                            new TrackEndedEventArgs(connection, currentTrack, TrackEndReason.Stopped),
                            message.Channel);
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
                    await completed.Task;
            }

            if (!sessions.IsCurrent(session))
                throw new PlaybackSessionExpiredException();

            logger.LogInformation(
                "Skip requested for guild {GuildId}. Current playback stopped.",
                guildId);

            return PlaybackControlResult.Succeeded(
                localizationService.Get(
                    guildId,
                    LocalizationKeys.SkipCommandResponse));
        }
        catch (PlaybackSessionExpiredException)
        {
            return PlaybackControlResult.Failed(
                "Playback session expired.",
                ValidationErrorKeys.PlaybackSuperseded);
        }
        catch (PlaybackOperationBusyException)
        {
            return PlaybackControlResult.Failed(
                "Another playback operation is active.",
                ValidationErrorKeys.PlaybackBusy);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "SkipAsync");

            return PlaybackControlResult.Failed(
                "Lavalink operation failed.",
                ValidationErrorKeys.LavalinkError);
        }
        finally
        {
            if (reservation is not null)
                sessions.ReleaseControl(reservation);
        }
    }

    public async Task<PlaybackControlResult> PreviousAsync(
        IDiscordMessage message,
        IDiscordMember? member)
    {
        var (connection, channel, guildId, valid) =
            await playerConnectionService.TryGetAndValidateExistingPlayerAsync(
                message,
                member?.VoiceState?.Channel);

        if (!valid || connection is null || channel is null)
        {
            return PlaybackControlResult.Failed(
                "Could not resolve an active Lavalink player.",
                ValidationErrorKeys.BotIsNotConnectedError);
        }

        var session = sessions.Capture(guildId, connection);

        if (session is null)
        {
            return PlaybackControlResult.Failed(
                "Playback session expired.",
                ValidationErrorKeys.PlaybackSuperseded);
        }

        ControlReservation? reservation = null;

        try
        {
            reservation = sessions.ReserveControl(session, "Previous");

            var previous = await queueRepository.GetPreviousItemAsync(guildId);

            sessions.ValidateControl(session, reservation);

            if (previous is null)
            {
                return PlaybackControlResult.Failed(
                    localizationService.Get(
                        guildId,
                        LocalizationKeys.PreviousCommandError),
                    LocalizationKeys.PreviousCommandError);
            }

            var result = await trackPlaybackService.StartExistingAsync(
                session,
                message.Channel,
                previous.Id,
                PlaybackStartReason.Previous,
                reservation: reservation);

            return result.Status switch
            {
                PlaybackStartStatus.Started =>
                    PlaybackControlResult.Succeeded(
                        "Previous track started."),

                PlaybackStartStatus.StartedStatePending =>
                    new PlaybackControlResult(
                        true,
                        "Playback started; state reconciliation is pending.",
                        ValidationErrorKeys.PlaybackStatePending),

                PlaybackStartStatus.Unknown =>
                    new PlaybackControlResult(
                        true,
                        "Playback outcome is being reconciled.",
                        ValidationErrorKeys.PlaybackOutcomeUnknown),

                PlaybackStartStatus.Superseded =>
                    PlaybackControlResult.Failed(
                        "Previous track was superseded.",
                        ValidationErrorKeys.PlaybackSuperseded),

                PlaybackStartStatus.Busy =>
                    PlaybackControlResult.Failed(
                        "Another playback operation is active.",
                        ValidationErrorKeys.PlaybackBusy),

                PlaybackStartStatus.Empty =>
                    PlaybackControlResult.Failed(
                        localizationService.Get(
                            guildId,
                            LocalizationKeys.PreviousCommandError),
                        LocalizationKeys.PreviousCommandError),

                _ =>
                    PlaybackControlResult.Failed(
                        "Previous track was not started.",
                        ValidationErrorKeys.LavalinkError)
            };
        }
        catch (PlaybackSessionExpiredException)
        {
            return PlaybackControlResult.Failed(
                "Playback session expired.",
                ValidationErrorKeys.PlaybackSuperseded);
        }
        catch (PlaybackOperationBusyException)
        {
            return PlaybackControlResult.Failed(
                "Another playback operation is active.",
                ValidationErrorKeys.PlaybackBusy);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "PreviousAsync");

            return PlaybackControlResult.Failed(
                "Lavalink operation failed.",
                ValidationErrorKeys.LavalinkError);
        }
        finally
        {
            if (reservation is not null)
                sessions.ReleaseControl(reservation);
        }
    }

    public async Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member)
    {
        var channel = member?.VoiceState?.Channel;
        if (channel is null)
        {
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.UserNotInVoiceChannel);
            return;
        }

        var guildId = channel.Guild.Id;
        var leaving = sessions.LeaveAsync(guildId, player => playbackCleanupService.CleanupAsync(guildId, player, joinWasNeverInvoked: false));
       
        try
        {
            var result = await leaving.WaitAsync(lifecycleOptions.Value.LeaveDeadline);
            if (!result.FullySucceeded)
            {
                await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            }
        }
        catch (TimeoutException ex)
        {
            logger.LogWarning(ex, "Leave cleanup is pending. Guild={GuildId}", guildId);
            _ = ObserveLeaveAsync(leaving);
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Leave cleanup failed. Guild={GuildId}", guildId);
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
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

    private async Task<PlaybackControlResult> ExecuteSessionBoundControlAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string operationName,
        Func<Lavalink4NET.Players.ILavalinkPlayer, ValueTask> transportOperation,
        Func<ulong, Lavalink4NET.Players.ILavalinkPlayer, Task>? persistenceOperation,
        string noTrackLocalizationKey,
        string successLocalizationKey,
        string noTrackErrorCode)
    {
        var (connection, channel, guildId, isValid) =
            await playerConnectionService.TryGetAndValidateExistingPlayerAsync(
                message,
                member?.VoiceState?.Channel);

        if (!isValid || connection is null || channel is null)
        {
            return PlaybackControlResult.Failed(
                "Could not resolve an active Lavalink player.",
                ValidationErrorKeys.BotIsNotConnectedError);
        }

        var session = sessions.Capture(guildId, connection);

        if (session is null)
        {
            return PlaybackControlResult.Failed(
                "Playback session expired.",
                ValidationErrorKeys.PlaybackSuperseded);
        }

        ControlReservation? reservation = null;

        try
        {
            reservation = sessions.ReserveControl(session, operationName);

            // Check the track after acquiring the shared reservation.
            if (connection.CurrentTrack is null)
            {
                await trackNotificationService.SendSafeAsync(
                    channel,
                    localizationService.Get(guildId, noTrackLocalizationKey),
                    $"{operationName}.NoTrack");

                return PlaybackControlResult.Failed(
                    localizationService.Get(guildId, noTrackLocalizationKey),
                    noTrackErrorCode);
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

            return PlaybackControlResult.Succeeded(
                localizationService.Get(guildId, successLocalizationKey));
        }
        catch (PlaybackSessionExpiredException)
        {
            return PlaybackControlResult.Failed(
                "Playback session expired.",
                ValidationErrorKeys.PlaybackSuperseded);
        }
        catch (PlaybackOperationBusyException)
        {
            return PlaybackControlResult.Failed(
                "Another playback operation is active.",
                ValidationErrorKeys.PlaybackBusy);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, operationName);

            return PlaybackControlResult.Failed(
                "Lavalink operation failed.",
                ValidationErrorKeys.LavalinkError);
        }
        finally
        {
            if (reservation is not null)
                sessions.ReleaseControl(reservation);
        }
    }
}
