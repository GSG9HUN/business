using DC_bot.Interface.Service.Music;
using Lavalink4NET.Protocol.Payloads.Events;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace DC_bot_tests.EndToEndTests.Service;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class MusicFlowEndToEndTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task MusicCommands_WithRealDiscordAndLavalink_PlayPauseResumeSkipLeave()
    {
        var context = await LiveMusicFlowTestContext.TryCreateAsync(testOutputHelper);
        if (context is null) return;
        await using var liveContext = context;

        await context.ExecuteCommandAsync("play", $"!play {EndToEndTestConfiguration.GetMusicTestQuery()}");

        if (context.Message.TextResponses.Count > 0)
        {
            throw new InvalidOperationException(
                "The play command returned a validation response before playback started: " +
                string.Join(" | ", context.Message.TextResponses));
        }

        _ = await context.WaitForPlayerWithCurrentTrackAsync();

        var controlMessage = await context.WaitForMusicControlMessageAsync();
        var initialProgressDescription = controlMessage.Embeds.FirstOrDefault()?.Description ?? "";

        var updatedControlMessage = await context.WaitForControlMessageDescriptionChangeAsync(
            controlMessage.Id,
            initialProgressDescription);
        var updatedProgressDescription = updatedControlMessage.Embeds.FirstOrDefault()?.Description ?? "";
        Assert.NotEqual(initialProgressDescription, updatedProgressDescription);

        await context.ExecuteCommandAsync("pause", "!pause");

        await context.ExecuteCommandAsync("resume", "!resume");

        await context.ExecuteCommandAsync("skip", "!skip");

        await context.LeaveAsync();
    }

    [Fact]
    public async Task TrackEnded_WithQueuedTrack_AutoAdvancesToNextTrack()
    {
        var context = await LiveMusicFlowTestContext.TryCreateAsync(testOutputHelper);
        if (context is null) return;
        await using var liveContext = context;

        await context.ExecuteCommandAsync("play", $"!play {EndToEndTestConfiguration.GetMusicTestQuery()}");
        _ = await context.WaitForStoredCurrentTrackAsync();

        await context.ExecuteCommandAsync("play", $"!play {EndToEndTestConfiguration.GetSecondMusicTestQuery()}");
        var queuedTrack = await context.WaitForQueuedTrackAsync();

        await context.SimulateTrackEndedAsync(TrackEndReason.Finished);

        var currentTrack = await context.WaitForStoredCurrentTrackAsync(
            track => string.Equals(track.ToString(), queuedTrack.ToString(), StringComparison.Ordinal));
        Assert.Equal(queuedTrack.ToString(), currentTrack.ToString());
    }

    [Fact]
    public async Task RepeatCommand_WithRealPlayback_ReplaysCurrentTrackAfterTrackEnd()
    {
        var context = await LiveMusicFlowTestContext.TryCreateAsync(testOutputHelper);
        if (context is null) return;
        await using var liveContext = context;

        await context.ExecuteCommandAsync("play", $"!play {EndToEndTestConfiguration.GetMusicTestQuery()}");
        var originalTrack = await context.WaitForStoredCurrentTrackAsync();

        await context.ExecuteCommandAsync("repeat", "!repeat");
        Assert.True(await context.Provider.GetRequiredService<IRepeatService>().IsRepeatingAsync(context.GuildId));

        await context.SimulateTrackEndedAsync(TrackEndReason.Finished);

        var repeatedTrack = await context.WaitForStoredCurrentTrackAsync(
            track => string.Equals(track.ToString(), originalTrack.ToString(), StringComparison.Ordinal));
        Assert.Equal(originalTrack.ToString(), repeatedTrack.ToString());
    }

    [Fact]
    public async Task RepeatListCommand_WithRealPlayback_RequeuesSnapshotAfterQueueEnds()
    {
        var context = await LiveMusicFlowTestContext.TryCreateAsync(testOutputHelper);
        if (context is null) return;
        await using var liveContext = context;

        await context.ExecuteCommandAsync("play", $"!play {EndToEndTestConfiguration.GetMusicTestQuery()}");
        var firstTrack = await context.WaitForStoredCurrentTrackAsync();

        await context.ExecuteCommandAsync("play", $"!play {EndToEndTestConfiguration.GetSecondMusicTestQuery()}");
        var queuedTrack = await context.WaitForQueuedTrackAsync();

        await context.ExecuteCommandAsync("repeatList", "!repeatList");
        Assert.True(await context.Provider.GetRequiredService<IRepeatService>().IsRepeatingListAsync(context.GuildId));

        await context.SimulateTrackEndedAsync(TrackEndReason.Finished);
        var secondTrack = await context.WaitForStoredCurrentTrackAsync(
            track => string.Equals(track.ToString(), queuedTrack.ToString(), StringComparison.Ordinal));
        Assert.Equal(queuedTrack.ToString(), secondTrack.ToString());

        await context.SimulateTrackEndedAsync(TrackEndReason.Finished);
        var repeatedFirstTrack = await context.WaitForStoredCurrentTrackAsync(
            track => string.Equals(track.ToString(), firstTrack.ToString(), StringComparison.Ordinal));
        Assert.Equal(firstTrack.ToString(), repeatedFirstTrack.ToString());
        Assert.True(await context.Provider.GetRequiredService<IMusicQueueService>().HasTracks(context.GuildId));
    }

    [Fact]
    public async Task ReactionControls_WithRealPlayback_InvokeLavalinkOperations()
    {
        var context = await LiveMusicFlowTestContext.TryCreateAsync(testOutputHelper);
        if (context is null) return;
        await using var liveContext = context;

        await context.ExecuteCommandAsync("play", $"!play {EndToEndTestConfiguration.GetMusicTestQuery()}");
        var player = await context.WaitForPlayerWithCurrentTrackAsync();
        Assert.NotNull(player.CurrentTrack);

        _ = await context.WaitForMusicControlMessageAsync();

        await context.ExecuteReactionAddedAsync(":pause_button:");
        Assert.DoesNotContain(context.Message.TextResponses, IsValidationFailure);
        Assert.NotNull(player.CurrentTrack);

        await context.ExecuteReactionAddedAsync(":arrow_forward:");
        Assert.DoesNotContain(context.Message.TextResponses, IsValidationFailure);
        Assert.NotNull(player.CurrentTrack);

        await context.ExecuteReactionAddedAsync(":track_next:");
        await context.WaitForPlayerWithoutCurrentTrackAsync();
    }

    private static bool IsValidationFailure(string response)
    {
        return response.Contains("voice channel", StringComparison.OrdinalIgnoreCase) ||
               response.Contains("lavalink", StringComparison.OrdinalIgnoreCase) ||
               response.Contains("error", StringComparison.OrdinalIgnoreCase);
    }
}
