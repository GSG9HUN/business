using System.Collections.Immutable;
using DC_bot.Configuration;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.BotControl;
using DC_bot.Service.Music;
using DC_bot.Service.Music.MusicServices;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DC_bot_tests.UnitTests.Service.BotControl;

[Trait("Category", "Unit")]
public sealed class MobileSelectionPlaybackTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SelectedCanonicalUrl_LoadsDirectlyAndQueuesExactTracksInOrder(bool playlist)
    {
        const ulong guildId = 123;
        var url = playlist ? "https://www.youtube.com/playlist?list=selected" : "https://www.youtube.com/watch?v=selected";
        var resolver = new TrackSearchResolverService(Options.Create(new SearchResolverOptions()));
        Assert.True(MobilePlayInput.TryParse(url, null, resolver, out var input));
        LavalinkTrack Track(string id) => new()
        {
            Identifier = id, Title = id, Author = "Artist", Duration = TimeSpan.FromSeconds(100),
            Uri = new Uri($"https://www.youtube.com/watch?v={id}"), SourceName = "youtube"
        };
        var tracks = playlist ? new[] { Track("first"), Track("second") } : new[] { Track("selected") };
        var load = playlist
            ? new TrackLoadResult(tracks, new PlaylistInformation("Selected playlist", null, ImmutableDictionary<string, System.Text.Json.JsonElement>.Empty))
            : new TrackLoadResult(tracks[0], null);
        var audio = new Mock<IAudioService>();
        audio.Setup(a => a.Tracks.LoadTracksAsync(url, TrackSearchMode.None, default, It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<TrackLoadResult>(load));
        var guild = Mock.Of<IDiscordGuild>(g => g.Id == guildId);
        var voice = Mock.Of<IDiscordChannel>(c => c.Guild == guild && c.Id == 456);
        var text = Mock.Of<IDiscordChannel>(c => c.Guild == guild && c.Id == 789);
        var message = Mock.Of<IDiscordMessage>(m => m.Channel == text);
        var player = new Mock<ILavalinkPlayer>();
        player.SetupGet(p => p.CurrentTrack).Returns(Track("already-playing"));
        var connection = new Mock<IPlayerConnectionService>();
        connection.Setup(c => c.TryJoinAndValidateAsync(message, voice, It.IsAny<CancellationToken>()))
            .ReturnsAsync((player.Object, voice, guildId, true));
        var queued = new List<QueueTrackToEnqueue>();
        var queue = new Mock<IMusicQueueService>(MockBehavior.Strict);
        queue.Setup(q => q.EnqueueMany(guildId, It.IsAny<IReadOnlyCollection<QueueTrackToEnqueue>>()))
            .Callback<ulong, IReadOnlyCollection<QueueTrackToEnqueue>>((_, items) => queued.AddRange(items)).Returns(Task.CompletedTask);
        queue.Setup(q => q.Enqueue(guildId, It.IsAny<ILavaLinkTrack>(), It.IsAny<string?>(), It.IsAny<TrackSearchMode?>(), "User"))
            .Callback<ulong, ILavaLinkTrack, string?, TrackSearchMode?, string?>((_, track, source, mode, by) => queued.Add(new(track, source, mode, by)))
            .Returns(Task.CompletedTask);
        var notifications = Mock.Of<ITrackNotificationService>();
        var localization = Mock.Of<ILocalizationService>();
        var playback = new TrackPlaybackService(queue.Object, notifications, Mock.Of<ICurrentTrackService>(), localization, Mock.Of<ILogger<TrackPlaybackService>>());
        var service = new PlaybackRequestService(audio.Object, Mock.Of<IResponseBuilder>(), localization,
            notifications, connection.Object, Mock.Of<IPlaybackEventHandlerService>(), playback,
            Mock.Of<ILogger<PlaybackRequestService>>());

        await service.PlayAsyncUrl(voice, input!.Url!, message, input.SearchMode, "User");

        Assert.Equal(tracks.Select(t => t.Identifier), queued.Select(q => q.Track.ToLavalinkTrack().Identifier));
        Assert.All(queued, q => { Assert.Equal("User", q.RequestedBy); Assert.Equal(TrackSearchMode.None, q.SourceSearchMode); });
        audio.Verify(a => a.Tracks.LoadTracksAsync(url, TrackSearchMode.None, default, It.IsAny<CancellationToken>()), Times.Once);
        player.Verify(p => p.PlayAsync(It.IsAny<LavalinkTrack>(), It.IsAny<TrackPlayProperties>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
