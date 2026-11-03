using System.Collections.Concurrent;
using DC_bot.Configuration;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Events;
using DC_bot.Interface.Service.Music.Lifecycle;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Persistence.Guilds;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Service.BotControl;
using DC_bot.Service.Music.Lifecycle.Cleanup;
using DC_bot.Service.Music.Lifecycle.Models;
using DC_bot.Service.Music.Lifecycle.Sessions;
using DC_bot.Wrapper.Discord;
using DSharpPlus;
using Lavalink4NET;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Music.Lifecycle.PersistentVoice;

/// <summary>
/// Maintains enabled persistent voice connections and restores confirmed playback checkpoints.
/// </summary>
/// <remarks>
/// Periodic checkpoints approximate the last confirmed position. Missing channels, failed cleanup, and uncertain starts
/// defer recovery; no human listener is required. The application must explicitly run this worker and observe its lifetime.
/// </remarks>
/// <param name="discordClient">Provides available guilds and saved target channels.</param>
/// <param name="audioService">Provides registered Lavalink players.</param>
/// <param name="guildRepository">Reads persistent voice preferences.</param>
/// <param name="playbackRepository">Reads and saves playback checkpoints.</param>
/// <param name="queueRepository">Validates queue ownership and pending starts.</param>
/// <param name="connectionService">Creates validated, owned voice players.</param>
/// <param name="trackPlaybackService">Restarts existing queue items under playback ownership.</param>
/// <param name="eventHandlers">Registers correlated playback events.</param>
/// <param name="validationService">Checks actual voice readiness.</param>
/// <param name="sessions">Coordinates player generations and cleanup.</param>
/// <param name="cleanupService">Confirms old player termination.</param>
/// <param name="persistentVoiceGate">Serializes recovery, settings, and explicit leave.</param>
/// <param name="options">Provides the configured join and cleanup deadlines for each recovery pass.</param>
/// <param name="logger">Records recovery failures and results.</param>
public sealed class PersistentVoiceRecoveryService(
    DiscordClient discordClient,
    IAudioService audioService,
    IGuildDataRepository guildRepository,
    IPlaybackStateRepository playbackRepository,
    IQueueRepository queueRepository,
    IPlayerConnectionService connectionService,
    ITrackPlaybackService trackPlaybackService,
    IPlaybackEventHandlerService eventHandlers,
    IValidationService validationService,
    GuildPlaybackSessions sessions,
    PlaybackCleanupService cleanupService,
    PersistentVoiceGate persistentVoiceGate,
    IOptions<PlaybackLifecycleOptions> options,
    ILogger<PersistentVoiceRecoveryService> logger)
{
    private readonly ConcurrentDictionary<ulong, DateTimeOffset> _disconnectedSince = new();
    private readonly ConcurrentDictionary<ulong, DateTimeOffset> _retryAfter = new();
    private readonly ConcurrentDictionary<ulong, long> _restoreGeneration = new();

    /// <summary>
    /// Periodically reconciles available guilds until application shutdown.
    /// </summary>
    /// <param name="cancellationToken">Stops scheduling checks and waiting on supported work at shutdown.</param>
    /// <returns>The worker lifetime task; owned cleanup may outlive a cancelled check.</returns>
    /// <remarks>Checks every five seconds and does not require a human listener or an active track.</remarks>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        do
        {
            var jobs = discordClient.Guilds.Keys.Select(
                guildId => ReconcileSafelyAsync(guildId, cancellationToken));

            await Task.WhenAll(jobs);
        }
        while (await timer.WaitForNextTickAsync(cancellationToken));
    }

    /// <summary>
    /// Bounds one guild reconciliation and delays retries after failures.
    /// </summary>
    /// <param name="guildId">The guild selected from the available Discord guild cache.</param>
    /// <param name="cancellationToken">The application lifetime cancellation request.</param>
    /// <returns>A task that completes when the check finishes, is deferred, or loses ownership.</returns>
    private async Task ReconcileSafelyAsync(
        ulong guildId,
        CancellationToken cancellationToken)
    {
        if (_retryAfter.TryGetValue(guildId, out var retryAt)
            && retryAt > DateTimeOffset.UtcNow)
        {
            return;
        }

        var gate = persistentVoiceGate.Get(guildId);

        if (!await gate.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

            deadline.CancelAfter(options.Value.JoinDeadline + options.Value.LeaveDeadline);

            await ReconcileAsync(guildId, deadline.Token);
            _retryAfter.TryRemove(guildId, out _);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (PlaybackSessionExpiredException)
        {
            // A newer lifecycle operation owns the guild; re-evaluate on the next pass.
        }
        catch (PlaybackOperationBusyException)
        {
            _retryAfter[guildId] = DateTimeOffset.UtcNow.AddSeconds(5);
        }
        catch (Exception exception)
        {
            _retryAfter[guildId] = DateTimeOffset.UtcNow.AddSeconds(30);

            logger.LogWarning(
                exception,
                "Persistent voice recovery deferred. Guild={GuildId}",
                guildId);
        }
        finally
        {
            gate.Release();
        }
    }

    /// <summary>
    /// Preserves healthy players or creates a replacement after confirmed cleanup.
    /// </summary>
    /// <param name="guildId">The guild whose saved target and owned player are reconciled.</param>
    /// <param name="cancellationToken">Bounds channel resolution, validation, and waiting for cleanup or join.</param>
    /// <returns>A task that completes after preserving or restoring the guild connection.</returns>
    private async Task ReconcileAsync(
        ulong guildId,
        CancellationToken cancellationToken)
    {
        var settings = await guildRepository.GetTwentyFourSevenSettingsAsync(
            guildId,
            cancellationToken);

        if (!settings.IsEnabled)
        {
            _disconnectedSince.TryRemove(guildId, out _);
            _restoreGeneration.TryRemove(guildId, out _);
            return;
        }

        if (settings.VoiceChannelId is not { } voiceChannelId
            || settings.TextChannelId is not { } textChannelId
            || !discordClient.Guilds.TryGetValue(guildId, out var guild))
        {
            return;
        }

        var voice = guild.Channels.GetValueOrDefault(voiceChannelId)
            ?? await guild.GetChannelAsync(voiceChannelId).WaitAsync(cancellationToken);

        var text = guild.Channels.GetValueOrDefault(textChannelId)
            ?? await guild.GetChannelAsync(textChannelId).WaitAsync(cancellationToken);

        if (voice.GuildId != guildId || text.GuildId != guildId)
        {
            throw new InvalidOperationException(
                "Persistent voice targets belong to a different guild.");
        }

        IDiscordChannel voiceChannel = new DiscordChannelWrapper(
            voice,
            guild: guild);

        IDiscordChannel textChannel = new DiscordChannelWrapper(
            text,
            guild: guild);

        var registered = await audioService.Players.GetPlayerAsync(
            guildId,
            cancellationToken);

        var session = sessions.CaptureCurrent(guildId);

        if (session is not null
            && ReferenceEquals(registered, session.Player))
        {
            var healthy = await IsHealthyAsync(
                session,
                voiceChannelId,
                cancellationToken);

            if (healthy)
            {
                _disconnectedSince.TryRemove(guildId, out _);

                if (_restoreGeneration.TryGetValue(guildId, out var generation)
                    && generation == session.Generation)
                {
                    await RestoreAsync(
                        session,
                        textChannel,
                        cancellationToken);
                }

                await SaveCheckpointAsync(session, cancellationToken);
                return;
            }

            var firstFailure = _disconnectedSince.GetOrAdd(
                guildId,
                DateTimeOffset.UtcNow);

            if (DateTimeOffset.UtcNow - firstFailure < TimeSpan.FromSeconds(10))
            {
                return;
            }
        }

        // A pending start is not permission to blindly replay its track.
        var pending = await queueRepository.GetPendingStartsAsync(
            guildId,
            cancellationToken);

        if (pending.Count != 0)
        {
            throw new PlaybackOperationBusyException(
                "Pending playback must reconcile before persistent voice recovery.");
        }

        if (session is not null)
        {
            var cleanupTask = sessions.LeaveIfCurrentAsync(
                session,
                player => cleanupService.CleanupAsync(
                    guildId,
                    player,
                    joinWasNeverInvoked: false));

            _ = ObserveCleanupAsync(cleanupTask, guildId);
            var cleanup = await cleanupTask.WaitAsync(cancellationToken);

            if (!cleanup.FullySucceeded)
            {
                throw new InvalidOperationException(
                    "Old player cleanup has not fully settled.");
            }
        }
        else
        {
            var retry = sessions.RetryClosingCleanup(
                guildId,
                player => cleanupService.CleanupAsync(
                    guildId,
                    player,
                    joinWasNeverInvoked: false));

            if (retry is not null)
            {
                _ = ObserveCleanupAsync(retry, guildId);
                var cleanup = await retry.WaitAsync(cancellationToken);

                if (!cleanup.FullySucceeded)
                {
                    throw new InvalidOperationException(
                        "Quarantined cleanup has not fully settled.");
                }
            }
        }

        var message = new BotControlDiscordMessage(
            textChannel,
            new DiscordUserWrapper(discordClient.CurrentUser));

        var joined = await connectionService.TryJoinAndValidateAsync(
            message,
            voiceChannel,
            cancellationToken);

        if (!joined.isValid || joined.connection is null)
        {
            throw new InvalidOperationException(
                "Persistent voice join did not produce a valid player.");
        }

        var restoredSession = sessions.Capture(guildId, joined.connection)
            ?? throw new PlaybackSessionExpiredException();

        _restoreGeneration[guildId] = restoredSession.Generation;

        eventHandlers.RegisterPlaybackFinishedHandler(
            guildId,
            joined.connection,
            textChannel);

        await RestoreAsync(
            restoredSession,
            textChannel,
            cancellationToken);

        _disconnectedSince.TryRemove(guildId, out _);

        logger.LogInformation(
            "Persistent voice restored. Guild={GuildId} Channel={ChannelId}",
            guildId,
            voiceChannelId);
    }

    /// <summary>
    /// Refreshes owned player state without blocking the Discord event pipeline.
    /// </summary>
    /// <param name="session">The player generation whose transport must remain owned.</param>
    /// <param name="voiceChannelId">The saved voice channel expected for this guild.</param>
    /// <param name="cancellationToken">Cancels supported refresh and remote validation requests.</param>
    /// <returns>True when validation succeeds and the player targets the saved voice channel.</returns>
    private async Task<bool> IsHealthyAsync(
        PlaybackSession session,
        ulong voiceChannelId,
        CancellationToken cancellationToken)
    {
        return await sessions.RunTransportAsync(session, async () =>
        {
            await session.Player.RefreshAsync(cancellationToken);

            var result = await validationService.ValidateConnectionAsync(
                session.Player,
                cancellationToken);

            return result.IsValid
                && session.Player.VoiceChannelId == voiceChannelId;
        });
    }

    /// <summary>
    /// Saves position only for the ready track protected by the current control reservation.
    /// </summary>
    /// <param name="session">The current player generation with a confirmed track attempt.</param>
    /// <param name="cancellationToken">Cancels supported checkpoint persistence.</param>
    /// <returns>A task completing after a matching queue item is checkpointed or no eligible attempt remains.</returns>
    private async Task SaveCheckpointAsync(
        PlaybackSession session,
        CancellationToken cancellationToken)
    {
        var reservation = sessions.ReserveControl(
            session,
            "PersistentVoiceCheckpoint");

        try
        {
            var queueItemId = sessions.GetReadyQueueItemId(session);

            if (!queueItemId.HasValue)
            {
                return;
            }

            await sessions.MutateAsync(session, async () =>
            {
                sessions.ValidateControl(session, reservation);

                var state = await playbackRepository.GetOrCreateAsync(
                    session.GuildId,
                    cancellationToken);

                if (state.QueueItemId != queueItemId)
                {
                    return false;
                }

                var position = session.Player.Position?.Position
                    ?? TimeSpan.Zero;

                if (position < TimeSpan.Zero)
                {
                    position = TimeSpan.Zero;
                }

                var duration = session.Player.CurrentTrack?.Duration;

                if (duration is { } length
                    && length > TimeSpan.Zero
                    && position > length)
                {
                    position = length;
                }

                await playbackRepository.SetPlaybackPositionAsync(
                    session.GuildId,
                    position,
                    session.Player.State == PlayerState.Paused,
                    cancellationToken);

                return true;
            });
        }
        finally
        {
            sessions.ReleaseControl(reservation);
        }
    }

    /// <summary>
    /// Restarts only the confirmed playing queue item retained by recovery cleanup.
    /// </summary>
    /// <param name="session">The replacement player generation receiving the retained queue item.</param>
    /// <param name="textChannel">The saved channel used for subsequent playback events and notifications.</param>
    /// <param name="cancellationToken">Bounds supported checkpoint reads and owned start operations.</param>
    /// <returns>A task completing after confirmed restoration or an idle connection with no eligible checkpoint.</returns>
    /// <remarks>Unknown starts are not blindly replayed. Non-seekable tracks restart without a position.</remarks>
    private async Task RestoreAsync(
        PlaybackSession session,
        IDiscordChannel textChannel,
        CancellationToken cancellationToken)
    {
        var reservation = sessions.ReserveControl(
            session,
            "PersistentVoiceRestore");

        try
        {
            if (session.Player.CurrentTrack is not null)
            {
                _restoreGeneration.TryRemove(session.GuildId, out _);
                return;
            }

            var state = await playbackRepository.GetOrCreateAsync(
                session.GuildId,
                cancellationToken);

            if (state.QueueItemId is not { } queueItemId)
            {
                _restoreGeneration.TryRemove(session.GuildId, out _);
                return;
            }

            var item = await queueRepository.GetByIdAsync(
                queueItemId,
                cancellationToken);

            if (item is null
                || item.GuildId != session.GuildId
                || item.State != QueueItemState.Playing)
            {
                _restoreGeneration.TryRemove(session.GuildId, out _);
                return;
            }

            var track = Lavalink4NET.Tracks.LavalinkTrack.Parse(
                item.TrackIdentifier,
                null);

            var position = track.IsSeekable
                ? TimeSpan.FromSeconds(Math.Max(0, state.PositionSeconds))
                : TimeSpan.Zero;

            if (track.Duration > TimeSpan.Zero
                && position >= track.Duration)
            {
                position = TimeSpan.FromMilliseconds(
                    Math.Max(0, track.Duration.TotalMilliseconds - 1));
            }

            var result = await trackPlaybackService.StartExistingAsync(
                session,
                textChannel,
                queueItemId,
                PlaybackStartReason.Recovery,
                reservation: reservation,
                cancellationToken: cancellationToken,
                resume: new PlaybackResumeOptions(
                    position,
                    state.IsPaused));

            if (result.Status == PlaybackStartStatus.Started)
            {
                _restoreGeneration.TryRemove(session.GuildId, out _);
                return;
            }

            throw new PlaybackOperationBusyException(
                $"Playback restoration requires reconciliation: {result.Status}.");
        }
        finally
        {
            sessions.ReleaseControl(reservation);
        }
    }

    /// <summary>
    /// Observes owned cleanup after a recovery caller has stopped waiting.
    /// </summary>
    /// <param name="cleanup">The lifecycle-owned task that must finish independently of caller cancellation.</param>
    /// <param name="guildId">The guild included in late-failure diagnostics.</param>
    /// <returns>A task completing after the late cleanup outcome has been observed.</returns>
    private async Task ObserveCleanupAsync(Task<LeaveResult> cleanup, ulong guildId)
    {
        try
        {
            await cleanup.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Persistent voice cleanup failed after admission. Guild={GuildId}", guildId);
        }
    }
}
