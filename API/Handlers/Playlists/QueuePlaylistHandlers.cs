using API.Mapping;
using API.Requests.Playlists;
using API.Responses.Playlists;
using DC_bot.Configuration;
using DC_bot.Interface.Service.Persistence.Exceptions;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Models.Playlists;
using DC_bot.Interface.Service.Persistence.Playlists;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Validation;
using Microsoft.Extensions.Options;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers.Playlists;

/// <summary>Creates atomic saved playlists from immutable, authorized queue-occurrence snapshots.</summary>
public static class QueuePlaylistHandlers
{

    /// <summary>Validates every captured occurrence before creating the playlist and all tracks in one transaction.</summary>
    public static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        CreateQueuePlaylistRequest request,
        IMobileAppUserRepository userRepository,
        IQueueRepository queueRepository,
        IPlaylistRepository playlistRepository,
        IOptions<PlaylistOptions> options,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var (_, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext, userRepository, guildId, cancellationToken);

        if (accessError is not null)
        {
            return accessError;
        }

        if (!PlaylistNameValidator.TryNormalize(request.PlaylistName, out var name))
        {
            return HttpResults.BadRequest(new { ErrorCode = "InvalidPlaylistName" });
        }

        var urls = request.TrackUrls?.ToArray();
        var ids = request.QueueItemIds?.ToArray();

        if (urls is null
            || ids is null
            || urls.Length == 0
            || ids.Length != urls.Length)
        {
            return HttpResults.BadRequest(new { ErrorCode = "InvalidQueueSnapshot" });
        }

        if (urls.Length > Math.Max(1, options.Value.MaxTracksPerPlaylist))
        {
            return HttpResults.BadRequest(new { ErrorCode = "TrackLimitExceeded" });
        }

        var parsedIds = new List<long>(ids.Length);

        foreach (var id in ids)
        {
            if (!long.TryParse(id, out var parsed) || parsed <= 0)
            {
                return HttpResults.BadRequest(new { ErrorCode = "InvalidQueueSnapshot" });
            }

            parsedIds.Add(parsed);
        }

        var occurrences = await queueRepository.GetSnapshotItemsAsync(guildId, parsedIds, cancellationToken);
        var lookup = occurrences.ToDictionary(item => item.Id);
        var tracks = new List<PlaylistTrackCreateRecord>(urls.Length);
        var duration = 0L;
        string? artwork = null;

        for (var index = 0; index < urls.Length; index++)
        {
            if (!lookup.TryGetValue(parsedIds[index], out var occurrence))
            {
                return HttpResults.BadRequest(new { ErrorCode = "QueueSnapshotUnavailable" });
            }

            var decoded = TrackResponseMapper.TryMapQueueTrack(occurrence.TrackIdentifier, index);

            if (decoded is null
                || !Uri.TryCreate(urls[index], UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                || string.IsNullOrWhiteSpace(uri.Host)
                || !string.IsNullOrEmpty(uri.UserInfo)
                || urls[index].Length > 1024
                || occurrence.TrackIdentifier.Length > 1024
                || !string.Equals(decoded.TrackUri, urls[index], StringComparison.Ordinal))
            {
                return HttpResults.BadRequest(new { ErrorCode = "InvalidSnapshotTrack", TrackNumber = index + 1 });
            }

            tracks.Add(new PlaylistTrackCreateRecord(
                "Queue", occurrence.TrackIdentifier, urls[index]));
            duration += decoded.Duration;
            artwork ??= decoded.ArtworkUri;
        }

        try
        {
            var id = await playlistRepository.CreateSnapshotAsync(
                guildId, name, tracks, options.Value.MaxPlaylistsPerGuild,
                options.Value.MaxTracksPerPlaylist, cancellationToken);

            if (id is null)
            {
                return HttpResults.Conflict(new { ErrorCode = "PlaylistLimitReached" });
            }

            return HttpResults.Created($"/api/playlists/{id}", new PlaylistSummaryResponse(
                id.Value.ToString(), name, tracks.Count, (int)Math.Min(duration, int.MaxValue), artwork));
        }
        catch (UniqueConstraintConflictException)
        {
            return HttpResults.Conflict(new { ErrorCode = "PlaylistAlreadyExists" });
        }
    }
}
