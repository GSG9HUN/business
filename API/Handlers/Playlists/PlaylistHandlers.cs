using System.Text.Json;
using API.Mapping;
using API.Requests.Playlists;
using API.Responses.Playlists;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.Exceptions;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Models.Playlists;
using DC_bot.Interface.Service.Persistence.Playlists;
using DC_bot.Validation;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers.Playlists;

public static class PlaylistHandlers
{
    public static async Task<IResult> ListAsync(
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IPlaylistRepository playlistRepository,
        IPlaylistTrackRepository playlistTrackRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var (_, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            guildId,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        var playlists = await playlistRepository.GetByGuildAsync(guildId, cancellationToken);
        var playlistIds = playlists.Select(playlist => playlist.Id).ToList();
        var tracksByPlaylistId = (await playlistTrackRepository.GetByPlaylistIdsOrderedAsync(
                playlistIds,
                cancellationToken))
            .GroupBy(track => track.PlaylistId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var response = new List<PlaylistSummaryResponse>(playlists.Count);

        foreach (var playlist in playlists)
        {
            tracksByPlaylistId.TryGetValue(playlist.Id, out var tracks);
            response.Add(new PlaylistSummaryResponse(
                playlist.Id.ToString(),
                playlist.Name,
                playlist.TrackCount,
                TrackResponseMapper.SumDurations(tracks ?? [])));
        }

        return HttpResults.Ok(response);
    }

    public static async Task<IResult> GetAsync(
        long playlistId,
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IPlaylistRepository playlistRepository,
        IPlaylistTrackRepository playlistTrackRepository,
        CancellationToken cancellationToken)
    {
        var (playlist, accessError) = await RequirePlaylistAccessAsync(
            playlistId,
            httpContext,
            userRepository,
            playlistRepository,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        var tracks = await playlistTrackRepository.GetByPlaylistIdOrderedAsync(playlist!.Id, cancellationToken);
        return HttpResults.Ok(MapDetail(playlist, tracks));
    }

    public static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        CreatePlaylistRequest request,
        IMobileAppUserRepository userRepository,
        IPlaylistRepository playlistRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var (_, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            guildId,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        if (!PlaylistNameValidator.TryNormalize(request.GetPlaylistName(), out var playlistName))
        {
            return HttpResults.BadRequest(new { ErrorMessage = "Invalid playlist name." });
        }

        if (await playlistRepository.ExistsAsync(guildId, playlistName, cancellationToken))
        {
            return HttpResults.Conflict(new { ErrorMessage = $"Playlist '{playlistName}' already exists." });
        }

        try
        {
            var playlistId = await playlistRepository.CreatePlaylistAsync(guildId, playlistName, cancellationToken);
            return HttpResults.Created(
                $"/api/playlists/{playlistId}",
                new PlaylistSummaryResponse(playlistId.ToString(), playlistName, 0, 0));
        }
        catch (UniqueConstraintConflictException)
        {
            return HttpResults.Conflict(new { ErrorMessage = $"Playlist '{playlistName}' already exists." });
        }
    }

    public static async Task<IResult> RenameAsync(
        long playlistId,
        RenamePlaylistRequest request,
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IPlaylistRepository playlistRepository,
        CancellationToken cancellationToken)
    {
        var (playlist, accessError) = await RequirePlaylistAccessAsync(
            playlistId,
            httpContext,
            userRepository,
            playlistRepository,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        if (!PlaylistNameValidator.TryNormalize(request.NewName, out var newName))
        {
            return HttpResults.BadRequest(new { ErrorMessage = "Invalid playlist name." });
        }

        if (await playlistRepository.ExistsAsync(playlist!.GuildId, newName, cancellationToken))
        {
            return HttpResults.Conflict(new { ErrorMessage = $"Playlist '{newName}' already exists." });
        }

        try
        {
            var renamed = await playlistRepository.RenameByIdAsync(playlist.Id, newName, cancellationToken);
            return renamed ? HttpResults.NoContent() : HttpResults.NotFound(new { ErrorMessage = "Playlist was not found." });
        }
        catch (UniqueConstraintConflictException)
        {
            return HttpResults.Conflict(new { ErrorMessage = $"Playlist '{newName}' already exists." });
        }
    }

    public static async Task<IResult> DeleteAsync(
        long playlistId,
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IPlaylistRepository playlistRepository,
        CancellationToken cancellationToken)
    {
        var (playlist, accessError) = await RequirePlaylistAccessAsync(
            playlistId,
            httpContext,
            userRepository,
            playlistRepository,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        var deleted = await playlistRepository.DeleteByIdAsync(playlist!.Id, cancellationToken);
        return deleted ? HttpResults.NoContent() : HttpResults.NotFound(new { ErrorMessage = "Playlist was not found." });
    }

    public static async Task<IResult> RemoveTrackAsync(
        long playlistId,
        int trackNumber,
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IPlaylistRepository playlistRepository,
        IPlaylistTrackRepository playlistTrackRepository,
        CancellationToken cancellationToken)
    {
        var (playlist, accessError) = await RequirePlaylistAccessAsync(
            playlistId,
            httpContext,
            userRepository,
            playlistRepository,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        if (trackNumber <= 0)
        {
            return HttpResults.BadRequest(new { ErrorMessage = "Invalid track number." });
        }

        var tracks = await playlistTrackRepository.GetByPlaylistIdOrderedAsync(playlist!.Id, cancellationToken);
        if (tracks.All(track => track.OrderNumber != trackNumber))
        {
            return HttpResults.NotFound(new { ErrorMessage = "Playlist track was not found." });
        }

        await playlistTrackRepository.RemoveTrackAsync(playlist.Id, trackNumber, cancellationToken);
        return HttpResults.NoContent();
    }

    public static async Task<IResult> AddTrackAsync(
        long playlistId,
        AddSongRequest request,
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IPlaylistRepository playlistRepository,
        IBotControlCommandsRepository commandsRepository,
        CancellationToken cancellationToken)
    {
        var (playlist, accessError) = await RequirePlaylistAccessAsync(
            playlistId,
            httpContext,
            userRepository,
            playlistRepository,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        if (string.IsNullOrWhiteSpace(request.SongUrl))
        {
            return HttpResults.BadRequest(new { ErrorMessage = "Song URL is required." });
        }

        var discordUserId = ApiUserContext.TryGetDiscordUserId(httpContext, out var userId) ? userId : 0;
        var payloadJson = JsonSerializer.Serialize(new PlaylistTrackCommandPayload(
            playlist!.Id.ToString(),
            playlist.Name,
            request.SongUrl.Trim()));

        var command = await commandsRepository.EnqueueAsync(
            playlist.GuildId,
            discordUserId,
            "addSong",
            payloadJson,
            cancellationToken);

        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    public static async Task<IResult> LoadAsync(
        long playlistId,
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IPlaylistRepository playlistRepository,
        IBotControlCommandsRepository commandsRepository,
        CancellationToken cancellationToken)
    {
        var (playlist, accessError) = await RequirePlaylistAccessAsync(
            playlistId,
            httpContext,
            userRepository,
            playlistRepository,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        var discordUserId = ApiUserContext.TryGetDiscordUserId(httpContext, out var userId) ? userId : 0;
        var payloadJson = JsonSerializer.Serialize(new PlaylistCommandPayload(
            playlist!.Id.ToString(),
            playlist.Name));

        var command = await commandsRepository.EnqueueAsync(
            playlist.GuildId,
            discordUserId,
            "loadPlaylist",
            payloadJson,
            cancellationToken);

        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    public static async Task<IResult> ImportAsync(
        HttpContext httpContext,
        SavePlaylistRequest request,
        IMobileAppUserRepository userRepository,
        IBotControlCommandsRepository commandsRepository,
        CancellationToken cancellationToken)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var (discordUserId, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            guildId,
            cancellationToken);
        if (accessError is not null)
        {
            return accessError;
        }

        if (!PlaylistNameValidator.TryNormalize(request.Name, out var playlistName))
        {
            return HttpResults.BadRequest(new { ErrorMessage = "Invalid playlist name." });
        }

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return HttpResults.BadRequest(new { ErrorMessage = "Playlist URL is required." });
        }

        var payloadJson = JsonSerializer.Serialize(new PlaylistImportCommandPayload(
            playlistName,
            request.Url.Trim()));

        var command = await commandsRepository.EnqueueAsync(
            guildId,
            discordUserId,
            "savePlaylist",
            payloadJson,
            cancellationToken);

        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    private static async Task<(PlaylistRecord? Playlist, IResult? Error)> RequirePlaylistAccessAsync(
        long playlistId,
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IPlaylistRepository playlistRepository,
        CancellationToken cancellationToken)
    {
        if (playlistId <= 0)
        {
            return (null, HttpResults.BadRequest(new { ErrorMessage = "Invalid playlist id." }));
        }

        var playlist = await playlistRepository.GetByIdAsync(playlistId, cancellationToken);
        if (playlist is null)
        {
            return (null, HttpResults.NotFound(new { ErrorMessage = "Playlist was not found." }));
        }

        var (_, accessError) = await ApiUserContext.RequireGuildAccessAsync(
            httpContext,
            userRepository,
            playlist.GuildId,
            cancellationToken);

        return accessError is null ? (playlist, null) : (null, accessError);
    }

    private static PlaylistDetailResponse MapDetail(
        PlaylistRecord playlist,
        IReadOnlyList<PlaylistTrackRecord> tracks)
    {
        var responseTracks = tracks
            .Select(TrackResponseMapper.TryMapPlaylistTrack)
            .Where(track => track is not null)
            .Cast<PlaylistTrackResponse>()
            .ToList();

        return new PlaylistDetailResponse(
            playlist.Id.ToString(),
            playlist.GuildId.ToString(),
            playlist.Name,
            tracks.Count,
            TrackResponseMapper.SumDurations(tracks),
            responseTracks);
    }

    private sealed record PlaylistCommandPayload(string PlaylistId, string PlaylistName);

    private sealed record PlaylistTrackCommandPayload(string PlaylistId, string PlaylistName, string SongUrl);

    private sealed record PlaylistImportCommandPayload(string Name, string Url);
}
