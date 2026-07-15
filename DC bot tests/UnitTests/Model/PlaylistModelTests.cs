using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Persistence.Entities;

namespace DC_bot_tests.UnitTests.Model;

[Trait("Category", "Unit")]
public class PlaylistModelTests
{
    [Fact]
    public void ResultEnums_ShouldContainExpectedContractValues()
    {
        Assert.Equal(
            ["Added", "PlaylistDoesNotExist", "NoTracksFound", "InvalidSongUrl", "UnknownError"],
            Enum.GetNames<AddSongResult>());
        Assert.Equal(
            ["Created", "PlaylistAlreadyExists", "InvalidPlaylistName", "UnknownError"],
            Enum.GetNames<CreatePlaylistResult>());
        Assert.Equal(
            ["Deleted", "DoesNotExist", "UnknownError"],
            Enum.GetNames<DeletePlaylistResult>());
        Assert.Equal(
            ["Listed", "NoPlaylists", "UnknownError"],
            Enum.GetNames<ListPlaylistsStatus>());
        Assert.Equal(
            ["Removed", "PlaylistDoesNotExist", "SongNotFound", "InvalidPlaylistName", "InvalidTrackNumber", "UnknownError"],
            Enum.GetNames<RemoveSongResult>());
        Assert.Equal(
            ["Renamed", "PlaylistDoesNotExist", "PlaylistAlreadyExists", "InvalidPlaylistName", "UnknownError"],
            Enum.GetNames<RenamePlaylistResult>());
        Assert.Equal(
            ["Saved", "AlreadyExists", "NoTracksFound", "UnknownError"],
            Enum.GetNames<SavePlaylistResult>());
        Assert.Equal(
            ["Viewed", "PlaylistDoesNotExist", "EmptyPlaylist", "UnknownError"],
            Enum.GetNames<ViewPlaylistStatus>());
    }

    [Fact]
    public void PlaylistDtos_ShouldPreserveConstructorValues()
    {
        var playlistTrack = new PlaylistTrackDto(1, "yt", "track-id", "https://example.com/track");
        var playlist = new PlaylistDto("roadtrip", [playlistTrack]);
        var summary = new PlaylistSummaryDto("roadtrip", 1);
        var listResult = new ListPlaylistsResult(ListPlaylistsStatus.Listed, [summary]);
        var viewTrack = new PlaylistViewTrackDto(
            1,
            "Track title",
            "Track author",
            TimeSpan.FromMinutes(3),
            "https://example.com/track");
        var viewResult = new ViewPlaylistResult(ViewPlaylistStatus.Viewed, "roadtrip", [viewTrack]);

        Assert.Equal("roadtrip", playlist.Name);
        Assert.Same(playlistTrack, playlist.Tracks.Single());
        Assert.Equal(1, playlistTrack.OrderNumber);
        Assert.Equal("yt", playlistTrack.Source);
        Assert.Equal("track-id", playlistTrack.TrackIdentifier);
        Assert.Equal("https://example.com/track", playlistTrack.TrackUri);

        Assert.Equal(ListPlaylistsStatus.Listed, listResult.Status);
        Assert.Equal("roadtrip", listResult.Playlists.Single().Name);
        Assert.Equal(1, listResult.Playlists.Single().TrackCount);

        Assert.Equal(ViewPlaylistStatus.Viewed, viewResult.Status);
        Assert.Equal("roadtrip", viewResult.PlaylistName);
        Assert.Equal("Track title", viewResult.Tracks.Single().Title);
        Assert.Equal("Track author", viewResult.Tracks.Single().Author);
        Assert.Equal(TimeSpan.FromMinutes(3), viewResult.Tracks.Single().Duration);
    }

    [Fact]
    public void PersistenceRecords_ShouldPreserveConstructorValues()
    {
        var playlist = new PlaylistRecord(10, 123UL, "roadtrip");
        var summary = new PlaylistSummaryRecord(10, 123UL, "roadtrip", 2);
        var createTrack = new PlaylistTrackCreateRecord("yt", "track-id", "https://example.com/track");
        var track = new PlaylistTrackRecord(20, 10, 1, "yt", "track-id", "https://example.com/track");

        Assert.Equal(10, playlist.Id);
        Assert.Equal(123UL, playlist.GuildId);
        Assert.Equal("roadtrip", playlist.Name);

        Assert.Equal(10, summary.Id);
        Assert.Equal(123UL, summary.GuildId);
        Assert.Equal("roadtrip", summary.Name);
        Assert.Equal(2, summary.TrackCount);

        Assert.Equal("yt", createTrack.Source);
        Assert.Equal("track-id", createTrack.TrackIdentifier);
        Assert.Equal("https://example.com/track", createTrack.TrackUri);

        Assert.Equal(20, track.Id);
        Assert.Equal(10, track.PlaylistId);
        Assert.Equal(1, track.OrderNumber);
        Assert.Equal("yt", track.Source);
        Assert.Equal("track-id", track.TrackIdentifier);
        Assert.Equal("https://example.com/track", track.TrackUri);
    }

    [Fact]
    public void PlaylistEntities_ShouldInitializeDefaults()
    {
        var playlist = new PlaylistEntity();
        var track = new PlaylistTrackEntity();

        Assert.Equal(string.Empty, playlist.Name);
        Assert.Empty(playlist.Tracks);
        Assert.Equal(string.Empty, track.Source);
        Assert.Equal(string.Empty, track.TrackIdentifier);
        Assert.Equal(string.Empty, track.TrackUri);
    }
}
