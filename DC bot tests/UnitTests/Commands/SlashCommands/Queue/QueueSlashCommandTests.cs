using DC_bot.Commands.SlashCommands.Queue;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.SlashCommands.Queue;

[Trait("Category", "Unit")]
public class QueueSlashCommandTests : SlashCommandTestBase
{
    [Fact]
    public async Task Queue_ShouldCreateInteractionContextAndDelegateToViewListCommand()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new QueueSlashCommand(executor.Object, contextFactory.Object);

        await command.Queue(dsharpContext);

        contextFactory.Verify(x => x.Create(dsharpContext), Times.Once);
        VerifyRequest(
            executor,
            "viewList",
            slashContext.Object,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);
    }

    [Fact]
    public async Task ExecuteAsync_WithTracks_ShouldReturnEmbedAfterDeferring()
    {
        var track = CreateTrack("Track A", "Artist A");
        MusicQueueServiceMock
            .Setup(service => service.ViewQueue(123UL))
            .ReturnsAsync([track]);
        var context = CreateContext();

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "viewList",
            context,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        Assert.True(context.IsDeferred);
        var embed = Assert.Single(context.EmbedResponses);
        Assert.Equal("Playlist", embed.Title);
        Assert.NotNull(embed.Fields);
        Assert.Contains(embed.Fields, field =>
            field is { Name: "Track A", Value: not null } &&
            field.Value.Contains("Artist A"));
    }

    [Fact]
    public async Task ExecuteAsync_WhenQueueIsEmpty_ShouldReturnValidationError()
    {
        MusicQueueServiceMock
            .Setup(service => service.ViewQueue(123UL))
            .ReturnsAsync([]);
        var context = CreateContext();

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "viewList",
            context,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        Assert.True(context.IsDeferred);
        Assert.Contains("Queue is empty.", context.TextResponses);
    }
}
