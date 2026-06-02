using DC_bot.Commands.SlashCommands.Queue;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.EndToEndTests.Commands.SlashCommands.Queue;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class RepeatSlashCommandEndToEndTests : SlashCommandPipelineEndToEndTestBase
{
    [Fact]
    public async Task RepeatTrackSlashCommand_ShouldRunThroughTextCommandPipeline()
    {
        RepeatServiceMock.Setup(service => service.IsRepeatingListAsync(123UL)).ReturnsAsync(false);
        RepeatServiceMock.Setup(service => service.IsRepeatingAsync(123UL)).ReturnsAsync(false);
        CurrentTrackServiceMock
            .Setup(service => service.GetCurrentTrackFormattedAsync(123UL, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Track A");
        var context = CreateContext();
        var command = new RepeatSlashCommand(SlashCommandExecutor, Mock.Of<ISlashInteractionContextFactory>());

        await command.ExecuteTrackAsync(context);

        Assert.True(context.IsDeferred);
        Assert.Contains("Repeat is on for : Track A", context.TextResponses);
        RepeatServiceMock.Verify(service => service.SetRepeatingAsync(123UL, true), Times.Once);
    }

    [Fact]
    public async Task RepeatListSlashCommand_ShouldRunThroughTextCommandPipeline()
    {
        var currentTrack = CreateTrack("Track A", "Artist A");
        var queuedTrack = CreateTrack("Track B", "Artist B");
        RepeatServiceMock.Setup(service => service.IsRepeatingAsync(123UL)).ReturnsAsync(false);
        RepeatServiceMock.Setup(service => service.IsRepeatingListAsync(123UL)).ReturnsAsync(false);
        CurrentTrackServiceMock
            .Setup(service => service.GetCurrentTrackAsync(123UL, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentTrack);
        MusicQueueServiceMock
            .Setup(service => service.ViewQueue(123UL))
            .ReturnsAsync([queuedTrack]);
        TrackFormatterServiceMock
            .Setup(service => service.FormatCurrentTrackListAsync(123UL))
            .ReturnsAsync("Track A\nTrack B");
        var context = CreateContext();
        var command = new RepeatSlashCommand(SlashCommandExecutor, Mock.Of<ISlashInteractionContextFactory>());

        await command.ExecuteListAsync(context);

        Assert.True(context.IsDeferred);
        Assert.Contains(context.TextResponses, response => response.Contains("Repeat is on for current list:"));
        RepeatServiceMock.Verify(service => service.SetRepeatingListAsync(123UL, true), Times.Once);
    }
}
