using DC_bot.BotControl;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

namespace DC_bot_tests.UnitTests.Service.BotControl;

/// <summary>Contains skipped test scaffolds for queued playlist command behavior.</summary>
[Trait("Category", "Unit")]
public sealed class PlaylistBotControlHandlerTests
{

    /// <summary>Scaffold for forwarding import cancellation.</summary>
    [Fact(Skip = "Test scaffold; implementation pending.")]
    public Task Import_ForwardsCancellation()
    {
        return Task.CompletedTask;
    }

    /// <summary>Scaffold for rejecting undecodable tracks before joining voice.</summary>
    [Fact(Skip = "Test scaffold; implementation pending.")]
    public Task Load_RejectsUndecodableTracksBeforeJoining()
    {
        return Task.CompletedTask;
    }

    /// <summary>Scaffold for resolving the current playlist name and reporting add-track outcomes.</summary>
    [Theory(Skip = "Test scaffold; implementation pending.")]
    [InlineData(AddSongResult.Added, true)]
    [InlineData(AddSongResult.TrackLimitReached, false)]
    public Task DispatchAddTrack_UsesCurrentPlaylistAndReportsOutcome(AddSongResult outcome, bool success)
    {
        return Task.CompletedTask;
    }

    /// <summary>Scaffold for dispatching imports and reporting service outcomes.</summary>
    [Theory(Skip = "Test scaffold; implementation pending.")]
    [InlineData(SavePlaylistResult.Saved, true)]
    [InlineData(SavePlaylistResult.PlaylistLimitReached, false)]
    public Task DispatchImport_ReportsServiceOutcome(SavePlaylistResult outcome, bool success)
    {
        return Task.CompletedTask;
    }

    /// <summary>Scaffold for rejecting playlists owned by another guild.</summary>
    [Fact(Skip = "Test scaffold; implementation pending.")]
    public Task AddTrack_RejectsCrossGuildPlaylist()
    {
        return Task.CompletedTask;
    }

    /// <summary>Scaffold for rejecting malformed persisted command payloads.</summary>
    [Theory(Skip = "Test scaffold; implementation pending.")]
    [InlineData(BotControlCommandTypes.AddSong)]
    [InlineData(BotControlCommandTypes.SavePlaylist)]
    public Task Dispatch_RejectsMalformedPayload(string type)
    {
        return Task.CompletedTask;
    }

    /// <summary>Scaffold for requiring a current voice channel before loading.</summary>
    [Fact(Skip = "Test scaffold; implementation pending.")]
    public Task DispatchLoad_RequiresVoiceChannel()
    {
        return Task.CompletedTask;
    }

    /// <summary>Scaffold for ordered enqueue and starting only idle players.</summary>
    [Theory(Skip = "Test scaffold; implementation pending.")]
    [InlineData(false)]
    [InlineData(true)]
    public Task Load_QueuesSavedTracksAndStartsOnlyIdlePlayer(bool alreadyPlaying)
    {
        return Task.CompletedTask;
    }
}
