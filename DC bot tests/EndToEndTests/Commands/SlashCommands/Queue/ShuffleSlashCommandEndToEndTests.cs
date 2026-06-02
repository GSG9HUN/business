using DC_bot.Commands.SlashCommands.Queue;
using DC_bot.Interface;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.EndToEndTests.Commands.SlashCommands.Queue;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class ShuffleSlashCommandEndToEndTests : SlashCommandPipelineEndToEndTestBase
{
    [Fact]
    public async Task ShuffleSlashCommand_ShouldRunThroughTextCommandPipeline()
    {
        MusicQueueServiceMock
            .Setup(service => service.GetQueue(123UL))
            .ReturnsAsync(new Queue<ILavaLinkTrack>(
            [
                CreateTrack("Track A", "Artist A"),
                CreateTrack("Track B", "Artist B")
            ]));
        var context = CreateContext();
        var command = new ShuffleSlashCommand(SlashCommandExecutor, Mock.Of<ISlashInteractionContextFactory>());

        await command.ExecuteAsync(context);

        Assert.True(context.IsDeferred);
        Assert.Contains("The list has been shuffled.", context.TextResponses);
        MusicQueueServiceMock.Verify(
            service => service.SetQueue(
                123UL,
                It.Is<Queue<ILavaLinkTrack>>(queue => queue.Count == 2)),
            Times.Once);
    }
}
