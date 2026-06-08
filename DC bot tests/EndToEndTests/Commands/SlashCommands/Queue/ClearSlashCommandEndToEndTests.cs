using DC_bot.Commands.SlashCommands.Queue;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.EndToEndTests.Commands.SlashCommands.Queue;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class ClearSlashCommandEndToEndTests : SlashCommandPipelineEndToEndTestBase
{
    [Fact]
    public async Task ClearSlashCommand_WhenConfirmed_ShouldRunThroughTextCommandPipeline()
    {
        var context = CreateContext();
        var command = new ClearSlashCommand(
            SlashCommandExecutor,
            Mock.Of<ISlashInteractionContextFactory>(),
            LocalizationServiceMock.Object);

        await command.ExecuteAsync(context, confirm: true);

        Assert.True(context.IsDeferred);
        Assert.Contains(context.TextResponses, response => response.Contains("Playlist cleared."));
        MusicQueueServiceMock.Verify(service => service.ClearQueue(123UL), Times.Once);
    }

    [Fact]
    public async Task ClearSlashCommand_WhenNotConfirmed_ShouldNotClearQueue()
    {
        var context = CreateContext();
        var command = new ClearSlashCommand(
            SlashCommandExecutor,
            Mock.Of<ISlashInteractionContextFactory>(),
            LocalizationServiceMock.Object);

        await command.ExecuteAsync(context, confirm: false);

        Assert.False(context.IsDeferred);
        Assert.Contains("Set confirm to true to clear the playlist.", context.TextResponses);
        MusicQueueServiceMock.Verify(service => service.ClearQueue(It.IsAny<ulong>()), Times.Never);
    }
}
