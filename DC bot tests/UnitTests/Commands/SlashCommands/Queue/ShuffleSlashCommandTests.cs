using DC_bot.Commands.SlashCommands.Queue;
using DC_bot.Interface;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.SlashCommands.Queue;

[Trait("Category", "Unit")]
public class ShuffleSlashCommandTests : SlashCommandTestBase
{
    [Fact]
    public async Task Shuffle_ShouldCreateInteractionContextAndDelegateToExecutor()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new ShuffleSlashCommand(executor.Object, contextFactory.Object);

        await command.Shuffle(dsharpContext);

        contextFactory.Verify(x => x.Create(dsharpContext), Times.Once);
        VerifyRequest(
            executor,
            "shuffle",
            slashContext.Object,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);
    }

    [Fact]
    public async Task ExecuteAsync_WhenQueueHasLessThanTwoTracks_ShouldReturnValidationError()
    {
        MusicQueueServiceMock
            .Setup(service => service.GetQueue(123UL))
            .ReturnsAsync(new Queue<ILavaLinkTrack>([CreateTrack("Track A", "Artist A")]));
        var context = CreateContext();

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "shuffle",
            context,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        Assert.True(context.IsDeferred);
        Assert.Contains("There is not enough music in queue.", context.TextResponses);
        MusicQueueServiceMock.Verify(
            service => service.SetQueue(It.IsAny<ulong>(), It.IsAny<Queue<ILavaLinkTrack>>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithQueue_ShouldShuffleQueue()
    {
        var firstTrack = CreateTrack("Track A", "Artist A");
        var secondTrack = CreateTrack("Track B", "Artist B");
        MusicQueueServiceMock
            .Setup(service => service.GetQueue(123UL))
            .ReturnsAsync(new Queue<ILavaLinkTrack>([firstTrack, secondTrack]));
        var context = CreateContext();

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "shuffle",
            context,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        Assert.True(context.IsDeferred);
        Assert.Contains("The list has been shuffled.", context.TextResponses);
        MusicQueueServiceMock.Verify(
            service => service.SetQueue(
                123UL,
                It.Is<Queue<ILavaLinkTrack>>(queue => queue.Count == 2)),
            Times.Once);
    }
}
