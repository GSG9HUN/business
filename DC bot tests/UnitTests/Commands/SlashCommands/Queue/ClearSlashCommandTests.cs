using DC_bot.Commands.SlashCommands.Queue;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.SlashCommands.Queue;

[Trait("Category", "Unit")]
public class ClearSlashCommandTests : SlashCommandTestBase
{
    [Fact]
    public async Task Clear_WhenConfirmed_ShouldCreateInteractionContextAndDelegateToExecutor()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new ClearSlashCommand(executor.Object, contextFactory.Object, LocalizationService);

        await command.Clear(dsharpContext, confirm: true);

        contextFactory.Verify(x => x.Create(dsharpContext), Times.Once);
        VerifyRequest(
            executor,
            "clear",
            slashContext.Object,
            requireGuild: true,
            defer: true);
    }

    [Fact]
    public async Task Clear_WhenNotConfirmed_ShouldReturnConfirmationMessageWithoutExecuting()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new TestSlashInteractionContext();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext);
        var command = new ClearSlashCommand(executor.Object, contextFactory.Object, LocalizationService);

        await command.Clear(dsharpContext, confirm: false);

        Assert.Contains("Set confirm to true to clear the playlist.", slashContext.TextResponses);
        executor.Verify(
            x => x.ExecuteAsync(It.IsAny<SlashCommandExecutionRequest>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldClearQueueAndRespondAfterDeferring()
    {
        var context = CreateContext();

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "clear",
            context,
            requireGuild: true,
            defer: true);

        Assert.True(context.IsDeferred);
        Assert.Contains(context.TextResponses, response => response.Contains("Playlist cleared."));
        MusicQueueServiceMock.Verify(service => service.ClearQueue(123UL), Times.Once);
    }
}
