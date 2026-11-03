using System.Text.Json;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.BotControl;
using DC_bot.Interface.Service.BotControl.Models;
using DC_bot.Interface.Service.Music.Events;
using DC_bot.Interface.Service.Music.Lifecycle;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Music.Queue;
using DC_bot.Interface.Service.Music.Queue.Models;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.BotControl.Payloads;
using DC_bot.Service.Music.Lifecycle.Sessions;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;
using DC_bot.Interface.Service.Persistence.Models.Playlists;
using DC_bot.Interface.Service.Persistence.Playlists;
using DC_bot.Validation;
using DC_bot.Exceptions.Music;

namespace DC_bot.Service.BotControl;

/// <summary>Executes queued playlist mutations and loads saved tracks through the playback lifecycle.</summary>
public sealed class PlaylistBotControlHandler(
    IPlaylistService playlistService,
    IPlaylistRepository playlistRepository,
    IPlaylistTrackRepository trackRepository,
    ITrackSerializer trackSerializer,
    IMusicQueueService queueService,
    IPlayerConnectionService connectionService,
    IPlaybackEventHandlerService eventHandlers,
    ITrackPlaybackService playbackService,
    IBotControlResultFactory resultFactory,
    GuildPlaybackSessions sessions)
{

    /// <summary>Adds a resolved track to the existing playlist identified by the queued request.</summary>
    public async Task<BotControlCommandResult> AddTrackAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var payload = Parse<PlaylistTrackCommandPayload>(command.PayloadJson);

        if (payload is null || string.IsNullOrWhiteSpace(payload.SongUrl))
        {
            return Failure(command, "InvalidPayload");
        }

        var playlist = await ResolvePlaylistAsync(command, payload.PlaylistId, cancellationToken);

        if (playlist is null)
        {
            return Failure(command, "PlaylistNotFound");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var result = await playlistService.AddSongToPlaylistAsync(command.GuildId, playlist.Name, payload.SongUrl.Trim(), cancellationToken);
        return result == AddSongResult.Added
            ? Success(command, "Track added to playlist.")
            : Failure(command, result.ToString());
    }

    /// <summary>Imports a source playlist using the existing playlist validation and limits.</summary>
    public async Task<BotControlCommandResult> ImportAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var payload = Parse<PlaylistImportCommandPayload>(command.PayloadJson);

        if (payload is null
            || !PlaylistNameValidator.TryNormalize(payload.Name, out var name)
            || string.IsNullOrWhiteSpace(payload.Url))
        {
            return Failure(command, "InvalidPayload");
        }

        var result = await playlistService.SavePlaylistAsync(command.GuildId, name, payload.Url.Trim(), cancellationToken);
        return result == SavePlaylistResult.Saved
            ? Success(command, "Playlist imported.")
            : Failure(command, result.ToString());
    }

    /// <summary>Validates saved tracks before joining voice, enqueuing, and starting an idle player.</summary>
    public async Task<BotControlCommandResult> LoadAsync(
        BotControlCommandRecord command,
        IDiscordMessage message,
        IDiscordChannel voiceChannel,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var payload = Parse<PlaylistCommandPayload>(command.PayloadJson);

        if (payload is null)
        {
            return Failure(command, "InvalidPayload");
        }

        var playlist = await ResolvePlaylistAsync(command, payload.PlaylistId, cancellationToken);

        if (playlist is null)
        {
            return Failure(command, "PlaylistNotFound");
        }

        var savedTracks = await trackRepository.GetByPlaylistIdOrderedAsync(playlist.Id, cancellationToken);

        if (savedTracks.Count == 0)
        {
            return Failure(command, "EmptyPlaylist");
        }

        List<QueueTrackToEnqueue> tracks;

        try
        {
            tracks = savedTracks.Select(track => new QueueTrackToEnqueue(
                    trackSerializer.Deserialize(track.TrackIdentifier),
                    SourceQuery: null, SourceSearchMode: null, RequestedBy: null))
                .ToList();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Failure(command, "InvalidPlaylistTrack");
        }

        var joined = await connectionService.TryJoinAndValidateAsync(message, voiceChannel, cancellationToken);

        if (!joined.isValid || joined.connection is null)
        {
            return Failure(command, "VoiceConnectionFailed");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var session = sessions.Capture(command.GuildId, joined.connection);

        if (session is null)
        {
            return Failure(command, "PlaybackSuperseded");
        }

        try
        {
            await sessions.MutateAsync(session, async () =>
            {
                await queueService.EnqueueMany(command.GuildId, tracks, cancellationToken);
                return true;
            });
        }
        catch (PlaybackSessionExpiredException)
        {
            return Failure(command, "PlaybackSuperseded");
        }

        eventHandlers.RegisterPlaybackFinishedHandler(command.GuildId, joined.connection, message.Channel);

        if (joined.connection.CurrentTrack is null)
        {
            var result = await playbackService.TryPlayNextTrackAsync(
                session,
                message.Channel,
                cancellationToken: cancellationToken);
            return CreatePlaybackResult(command, result);
        }

        return Success(command, "Playlist loaded.");
    }

    /// <summary>Maps an idle-player start outcome without reporting uncertain playback as confirmed command success.</summary>
    /// <param name="command">The durable playlist load command whose outcome is being recorded.</param>
    /// <param name="result">The playback outcome, including attempt and queue occurrence identifiers.</param>
    /// <returns>A terminal result with playback correlation and reconciliation metadata.</returns>
    private BotControlCommandResult CreatePlaybackResult(BotControlCommandRecord command, PlaybackStartResult result)
    {
        var data = new
        {
            playbackStatus = result.Status.ToString(),
            operationId = result.OperationId,
            queueItemId = result.QueueItemId,
            requiresReconciliation = result.Status is PlaybackStartStatus.Unknown
                or PlaybackStartStatus.StartedStatePending or PlaybackStartStatus.Busy
        };

        if (result.Status == PlaybackStartStatus.Started)
        {
            return resultFactory.Success(command, "Playlist loaded; playback started.", data, shouldNotifyDiscord: false);
        }

        var message = result.Status switch
        {
            PlaybackStartStatus.StartedStatePending =>
                "Playback started, but saving its state is not confirmed. Do not reload the playlist; reconcile playback state.",
            PlaybackStartStatus.Unknown =>
                "Playlist enqueued; playback outcome is not confirmed. Do not reload the playlist; reconcile playback state.",
            PlaybackStartStatus.Busy =>
                "Playlist enqueued; a previous playback operation needs reconciliation. Do not reload the playlist.",
            PlaybackStartStatus.Superseded => "A newer session or leave operation superseded playlist playback.",
            PlaybackStartStatus.Empty => "No queued track is available to start playlist playback.",
            _ => "Playlist enqueued, but playback did not start."
        };
        return resultFactory.Failure(command, message, result.Status.ToString(), data, shouldNotifyDiscord: false);
    }

    /// <summary>Resolves the current playlist name and rejects IDs belonging to another guild.</summary>
    private async Task<PlaylistRecord?> ResolvePlaylistAsync(
        BotControlCommandRecord command,
        string playlistId,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(playlistId, out var id) || id <= 0)
        {
            return null;
        }

        var playlist = await playlistRepository.GetByIdAsync(id, cancellationToken);
        return playlist?.GuildId == command.GuildId ? playlist : null;
    }

    /// <summary>Returns a terminal command failure without a Discord notification.</summary>
    private BotControlCommandResult Failure(BotControlCommandRecord command, string code)
    {
        return resultFactory.Failure(command, $"Playlist operation failed: {code}.", code, shouldNotifyDiscord: false);
    }

    /// <summary>Returns a terminal command success without duplicating Discord responses.</summary>
    private BotControlCommandResult Success(BotControlCommandRecord command, string message)
    {
        return resultFactory.Success(command, message, shouldNotifyDiscord: false);
    }

    /// <summary>Parses persisted command JSON without allowing malformed input to escape dispatch.</summary>
    private static T? Parse<T>(string? json) where T : class
    {
        try
        {
            return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
