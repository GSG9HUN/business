using DC_bot.BotControl;
using DC_bot.Constants;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Music.Presentation;
using DC_bot.Interface.Service.Music.Queue;
using DC_bot.Interface.Service.Music.Queue.Models;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Service.Music.Lifecycle;
using DC_bot.Wrapper.Lavalink;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Playback;

public sealed class TrackPlaybackService(
    IMusicQueueService musicQueueService,
    ITrackNotificationService trackNotificationService,
    ILocalizationService localizationService,
    IQueueRepository queueRepository,
    ITrackSerializer trackSerializer,
    PlaybackRecoveryService playbackRecoveryService,
    PlaybackRemoteProbe playbackRemoteProbe,
    GuildPlaybackSessions sessions,
    ILogger<TrackPlaybackService> logger) : ITrackPlaybackService
{
    public async Task<PlaybackStartResult> PlayTheFoundMusicAsync(
        TrackLoadResult searchQuery, PlaybackSession session,
        IDiscordChannel textChannel, string? requestedBy,
        string? sourceQuery, TrackSearchMode? sourceSearchMode)
    {
        var operationId = Guid.NewGuid();
        if (!sessions.IsCurrent(session))
            return new(PlaybackStartStatus.Superseded, operationId);
        var tracks = searchQuery.IsPlaylist
            ? searchQuery.Tracks.ToList()
            : searchQuery.Track is { } single
                ? [single]
                : [];
        if (tracks.Count == 0 || searchQuery.IsFailed)
            return new(PlaybackStartStatus.NotStarted, operationId);

        try
        {
            await sessions.MutateAsync(session, async () =>
            {
                await musicQueueService.EnqueueMany(session.GuildId,
                    tracks.Select(track => new QueueTrackToEnqueue(
                        new LavaLinkTrackWrapper(track),
                        searchQuery.IsPlaylist ? track.Uri?.ToString() : sourceQuery,
                        sourceSearchMode, requestedBy)).ToList());
                return true;
            });
        }
        catch (PlaybackSessionExpiredException)
        {
            return new(PlaybackStartStatus.Superseded, operationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Playback enqueue uncertain. OperationId={OperationId} Guild={GuildId} Phase=Enqueue Category=Persistence",
                operationId, session.GuildId);
            return new(PlaybackStartStatus.Unknown, operationId);
        }

        if (session.Player.CurrentTrack is null)
            return await TryPlayNextTrackAsync(session, textChannel, PlaybackStartReason.First);

        try
        {
            var text = tracks.Count > 1
                ? localizationService.Get(session.GuildId, LocalizationKeys.PlayCommandListAddedQueue)
                : $"{localizationService.Get(session.GuildId, LocalizationKeys.PlayCommandMusicAddedQueue)} {tracks[0].Author} - {tracks[0].Title}";
            await trackNotificationService.SendSafeAsync(textChannel, text, "PlaybackQueued");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Queue notification failed. OperationId={OperationId} Guild={GuildId} Phase=Notify Category=Notification",
                operationId, session.GuildId);
        }

        return new(PlaybackStartStatus.Queued, operationId);
    }

    public Task<PlaybackStartResult> TryPlayNextTrackAsync(
        PlaybackSession session, IDiscordChannel textChannel,
        PlaybackStartReason reason = PlaybackStartReason.QueueNext, ControlReservation? reservation = null)
    {
        return StartAsync(session, textChannel, null, reason, null, reservation);
    }

    public Task<PlaybackStartResult> StartExistingAsync(
        PlaybackSession session,
        IDiscordChannel textChannel,
        long queueItemId,
        PlaybackStartReason reason,
        ILavaLinkTrack? replacement = null,
        ControlReservation? reservation = null)
    {
        return StartAsync(session, textChannel, queueItemId, reason, replacement, reservation);
    }

    private async Task<PlaybackStartResult> StartAsync(
        PlaybackSession session, IDiscordChannel channel,
        long? requestedQueueItemId, PlaybackStartReason reason,
        ILavaLinkTrack? replacement, ControlReservation? reservation)
    {
        var operationId = Guid.NewGuid();
        ControlReservation? ownedReservation = null;

        try
        {
            if (reservation is null)
            {
                ownedReservation = sessions.ReserveControl(session, $"Start:{reason}");

                reservation = ownedReservation;
            }
            else
            {
                sessions.ValidateControl(session, reservation);
            }

            return await StartCoreAsync(session, channel, requestedQueueItemId, reason, replacement, reservation,
                operationId);
        }
        catch (PlaybackSessionExpiredException)
        {
            return new(
                PlaybackStartStatus.Superseded,
                operationId);
        }
        catch (PlaybackOperationBusyException)
        {
            return new(
                PlaybackStartStatus.Busy,
                operationId);
        }
        finally
        {
            // The caller releases an inherited reservation.
            if (ownedReservation is not null)
                sessions.ReleaseControl(ownedReservation);
        }
    }

    private async Task<PlaybackStartResult> StartCoreAsync(PlaybackSession session, IDiscordChannel channel,
        long? requestedQueueItemId,
        PlaybackStartReason reason,
        ILavaLinkTrack? replacement,
        ControlReservation reservation,
        Guid operationId)
    {
        QueueStartClaim? claim = null;
        var dispatched = false;
        var started = false;

        try
        {
            sessions.BeginAttempt(session, operationId, reservation, replace: reason == PlaybackStartReason.Previous);

            var selection = await sessions.MutateAsync(session, () => queueRepository.ClaimForStartAsync(session.GuildId, operationId, requestedQueueItemId));
           
            if (selection.Status != QueueClaimStatus.Claimed)
            {
                sessions.AbandonAttempt(session, operationId);
                return new(selection.Status == QueueClaimStatus.Empty ? PlaybackStartStatus.Empty : PlaybackStartStatus.Busy, operationId);
            }

            claim = selection.Claim ?? throw new InvalidOperationException("Claimed result has no claim.");

            ILavaLinkTrack track;
            try
            {
                track = replacement ?? trackSerializer.Deserialize(claim.Item.TrackIdentifier, claim.Item.Id);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Invalid queued track. Guild={GuildId} OperationId={OperationId}", session.GuildId, operationId);
                
                await queueRepository.ReleaseStartAsync(session.GuildId, claim.Item.Id, operationId, skip: true);
                sessions.AbandonAttempt(session, operationId);
                
                return new(PlaybackStartStatus.NotStarted, operationId, claim.Item.Id);
            }

            var serializedTrack = trackSerializer.Serialize(track);
            await sessions.RunTransportAsync(session, async () =>
            {
                var playableTrack = await track.ToLavalinkTrack().GetPlayableTrackAsync();
                var encodedTrack = playableTrack.ToString();
                var nodeSessionId = session.Player.SessionId;
                var previousEncodedTrack = session.Player.CurrentTrack?.ToString();

                if (string.IsNullOrWhiteSpace(nodeSessionId))
                {
                    throw new InvalidOperationException("Player has no node session.");
                }

                // Persist remote scope BEFORE any PlayAsync may leave this process.
                // Network I/O does not run inside the repository transaction.
                var bound = await sessions.MutateAsync(session, () => queueRepository.BindStartNodeAsync(session.GuildId, claim.Item.Id, operationId, playbackRemoteProbe.NodeAddress, nodeSessionId));
               
                if (!bound)
                {
                    throw new InvalidOperationException("Start claim ownership changed.");
                }

                sessions.BindAttempt(session, operationId, claim.Item.Id, encodedTrack);
                if (session.Player.SessionId != nodeSessionId)
                {
                    throw new InvalidOperationException("Node session changed before dispatch.");
                }

                dispatched = true;
                try
                {
                    await session.Player.PlayAsync(playableTrack);
                    started = true;
                }
                catch (Exception transportError)
                {
                    // Read-only, bounded confirmation. No second PlayAsync.
                    var confirmed = await playbackRecoveryService.TryConfirmStartAsync(session, reservation, operationId, claim.Item.Id, nodeSessionId, encodedTrack, previousEncodedTrack);
                    
                    if (!confirmed)
                    {
                        throw;
                    }

                    started = true;
                    logger.LogWarning(transportError,
                        "Remote start confirmed after transport error. Guild={GuildId} OperationId={OperationId}",
                        session.GuildId, operationId);
                }

                return true;
            });

            var committed = await playbackRecoveryService.CommitKnownStartAsync(session, claim.Item.Id, operationId, serializedTrack, reason);
            
            if (!committed)
            {
                return new(PlaybackStartStatus.StartedStatePending, operationId, claim.Item.Id);
            }
            
            if (sessions.IsCurrent(session))
            {
                try
                {
                    await trackNotificationService.NotifyNowPlayingAsync(channel, track, track.StartPosition ?? TimeSpan.Zero, track.Duration);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Playback notification failed. Guild={GuildId} OperationId={OperationId}",
                        session.GuildId, operationId);
                }
            }

            return new(PlaybackStartStatus.Started, operationId, claim.Item.Id);
        }
        catch (PlaybackOperationBusyException)
        {
            return new(PlaybackStartStatus.Busy, operationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Playback start incomplete. Guild={GuildId} OperationId={OperationId} Dispatched={Dispatched}",
                session.GuildId, operationId, dispatched);
            if (dispatched)
            {
                // No AbandonAttempt or ReleaseStart for an uncertain remote start.
                return new(started
                    ? PlaybackStartStatus.StartedStatePending
                    : PlaybackStartStatus.Unknown, operationId, claim?.Item.Id);
            }

            if (claim is not null)
            {
                try
                {
                    await queueRepository.ReleaseStartAsync(session.GuildId, claim.Item.Id, operationId, skip: false);
                }
                catch (Exception releaseError)
                {
                    logger.LogError(releaseError,
                        "Claim release uncertain. Guild={GuildId} OperationId={OperationId}",
                        session.GuildId, operationId);
                    return new(PlaybackStartStatus.Unknown, operationId, claim.Item.Id);
                }
            }

            sessions.AbandonAttempt(session, operationId);
            return new(ex is PlaybackSessionExpiredException
                    ? PlaybackStartStatus.Superseded
                    : PlaybackStartStatus.NotStarted,
                operationId, claim?.Item.Id);
        }
    }
}