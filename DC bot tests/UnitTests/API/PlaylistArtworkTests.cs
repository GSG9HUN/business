using System.Security.Claims;
using System.Text.Json;
using API.Handlers.Playlists;
using API.Responses.Playlists;
using API.Requests.Playlists;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Models.Playlists;
using DC_bot.Interface.Service.Persistence.Playlists;
using Lavalink4NET.Tracks;
using Microsoft.AspNetCore.Http;
using Moq;

namespace DC_bot_tests.UnitTests.API;

/// <summary>Verifies artwork in playlist list and detail responses using saved track metadata.</summary>
[Trait("Category", "Unit")]
public sealed class PlaylistArtworkTests
{
    private const string FirstArtwork = "https://example.com/first.jpg";
    private const string SecondArtwork = "https://example.com/second.jpg";

    /// <summary>Both responses choose the first available cover and preserve per-track artwork.</summary>
    [Theory]
    [InlineData(0, null)]
    [InlineData(1, null)]
    [InlineData(2, FirstArtwork)]
    [InlineData(3, SecondArtwork)]
    [InlineData(4, SecondArtwork)]
    [InlineData(5, null)]
    [InlineData(0, FirstArtwork, FirstArtwork)]
    [InlineData(2, SecondArtwork, SecondArtwork)]
    [InlineData(5, FirstArtwork, FirstArtwork)]
    public async Task ListAndDetail_ReturnFirstAvailableArtwork(
        int scenario,
        string? expectedArtwork,
        string? customArtwork = null)
    {
        IReadOnlyList<PlaylistTrackRecord> tracks = scenario switch
        {
            0 => [],
            1 => [CreateTrack(1, null)],
            2 => [CreateTrack(1, FirstArtwork), CreateTrack(2, SecondArtwork)],
            3 => [CreateTrack(1, null), CreateTrack(2, SecondArtwork)],
            4 => [new(1, 10, 1, "youtube", "invalid", "https://example.com/invalid"), CreateTrack(2, SecondArtwork)],
            _ => [new(1, 10, 1, "youtube", "invalid", "https://example.com/invalid")]
        };
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "42")], "test"))
        };
        context.Items["guildId"] = 7UL;
        var users = new Mock<IMobileAppUserRepository>();
        users.Setup(repository => repository.HasGuildAccessAsync(42, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var playlists = new Mock<IPlaylistRepository>();
        playlists.Setup(repository => repository.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaylistRecord(10, 7, "Mix", customArtwork));
        playlists.Setup(repository => repository.GetByGuildAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlaylistSummaryRecord> { new(10, 7, "Mix", tracks.Count, customArtwork) });
        var playlistTracks = new Mock<IPlaylistTrackRepository>();
        playlistTracks.Setup(repository => repository.GetByPlaylistIdOrderedAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tracks);
        playlistTracks.Setup(repository => repository.GetByPlaylistIdsOrderedAsync(
                It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tracks);

        var listResult = await PlaylistHandlers.ListAsync(
            context, users.Object, playlists.Object, playlistTracks.Object, CancellationToken.None);
        var list = Assert.IsAssignableFrom<IReadOnlyList<PlaylistSummaryResponse>>(
            Assert.IsAssignableFrom<IValueHttpResult>(listResult).Value);
        Assert.Equal(expectedArtwork, Assert.Single(list).ArtworkUri);

        var detailResult = await PlaylistHandlers.GetAsync(
            10, context, users.Object, playlists.Object, playlistTracks.Object, CancellationToken.None);
        var detail = Assert.IsType<PlaylistDetailResponse>(Assert.IsAssignableFrom<IValueHttpResult>(detailResult).Value);
        Assert.Equal(expectedArtwork, detail.ArtworkUri);
        Assert.Equal(tracks.Count, detail.SongCount);
        Assert.Equal(scenario is 4 or 5 ? tracks.Count - 1 : tracks.Count, detail.Tracks.Count);

        foreach (var track in detail.Tracks)
        {
            var expectedTrackArtwork = track.OrderNumber == 1 && scenario == 2 ? FirstArtwork
                : track.OrderNumber == 2 ? SecondArtwork : null;
            Assert.Equal(expectedTrackArtwork, track.ArtworkUri);
            Assert.Equal($"https://example.com/track/{track.OrderNumber}", track.TrackUri);
        }

        var json = JsonSerializer.SerializeToElement(detail, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal(expectedArtwork, json.GetProperty("artworkUri").GetString());
    }

    /// <summary>Cover updates validate input and require access before writing to the repository.</summary>
    [Theory]
    [InlineData("https://example.com/cover.jpg", true, 204)]
    [InlineData("  https://example.com/cover.jpg  ", true, 204)]
    [InlineData(null, true, 204)]
    [InlineData("file:///cover.jpg", true, 400)]
    [InlineData("/cover.jpg", true, 400)]
    [InlineData("", true, 400)]
    [InlineData("https://user:password@example.com/cover.jpg", true, 400)]
    [InlineData("https://example.com/cover.jpg", false, 403)]
    public async Task SetArtwork_ValidatesAndAuthorizesBeforeSaving(string? artworkUri, bool allowed, int statusCode)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "42")], "test"))
        };
        var users = new Mock<IMobileAppUserRepository>();
        users.Setup(repository => repository.HasGuildAccessAsync(42, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allowed);
        var playlists = new Mock<IPlaylistRepository>();
        playlists.Setup(repository => repository.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaylistRecord(10, 7, "Mix"));
        playlists.Setup(repository => repository.SetArtworkAsync(10, artworkUri == null ? null : artworkUri.Trim(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        using var cancellation = new CancellationTokenSource();

        var result = await PlaylistHandlers.SetArtworkAsync(
            10, new SetPlaylistArtworkRequest(artworkUri), context, users.Object, playlists.Object, cancellation.Token);

        if (allowed)
        {
            Assert.Equal(statusCode, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        }
        else
        {
            Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.ForbidHttpResult>(result);
        }

        playlists.Verify(repository => repository.SetArtworkAsync(
                10, artworkUri == null ? null : artworkUri.Trim(), cancellation.Token),
            statusCode == 204 ? Times.Once() : Times.Never());
    }

    /// <summary>Creates a persisted track with optional artwork in its encoded metadata.</summary>
    private static PlaylistTrackRecord CreateTrack(int orderNumber, string? artworkUri)
    {
        var track = new LavalinkTrack
        {
            Title = $"Track {orderNumber}",
            Author = "Artist",
            Identifier = $"track-{orderNumber}",
            Duration = TimeSpan.FromSeconds(120),
            Uri = new Uri($"https://example.com/track/{orderNumber}"),
            ArtworkUri = artworkUri is null ? null : new Uri(artworkUri),
            SourceName = "youtube",
            IsSeekable = true,
            IsLiveStream = false
        };
        return new PlaylistTrackRecord(
            orderNumber, 10, orderNumber, "youtube", track.ToString(), track.Uri.ToString());
    }
}
