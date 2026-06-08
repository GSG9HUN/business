using DC_bot.Commands.SlashCommands.Utility;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.SlashCommands.Utility;

[Trait("Category", "Unit")]
public class HelpSlashCommandTests : SlashCommandTestBase
{
    [Fact]
    public async Task Help_ShouldCreateInteractionContextAndDelegateToExecutor()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new HelpSlashCommand(executor.Object, contextFactory.Object);

        await command.Help(dsharpContext);

        contextFactory.Verify(x => x.Create(dsharpContext), Times.Once);
        VerifyRequest(executor, "help", slashContext.Object, requireGuild: true);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldListAvailableCommands()
    {
        var context = CreateContext();

        await ExecuteSlashAsync(SlashCommandExecutor, "help", context, requireGuild: true);

        var response = Assert.Single(context.TextResponses);
        Assert.Contains("Available commands:", response);
        Assert.Contains("ping : Replies with Pong!", response);
        Assert.Contains("play : Plays a song", response);
    }
}
