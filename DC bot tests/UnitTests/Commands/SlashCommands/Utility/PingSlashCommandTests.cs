using DC_bot.Commands.SlashCommands.Utility;
using DC_bot.Constants;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.SlashCommands.Utility;

[Trait("Category", "Unit")]
public class PingSlashCommandTests : SlashCommandTestBase
{
    [Fact]
    public async Task Ping_ShouldCreateInteractionContextAndDelegateToExecutor()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new PingSlashCommand(executor.Object, contextFactory.Object);

        await command.Ping(dsharpContext);

        contextFactory.Verify(x => x.Create(dsharpContext), Times.Once);
        VerifyRequest(executor, "ping", slashContext.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDelegateToExecutor()
    {
        var executor = CreateModuleExecutor();
        var context = CreateContext();
        var command = new PingSlashCommand(executor.Object, Mock.Of<ISlashInteractionContextFactory>());

        await command.ExecuteAsync(context);

        VerifyRequest(executor, "ping", context);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRespondWithPong()
    {
        var context = CreateContext();

        await ExecuteSlashAsync(SlashCommandExecutor, "ping", context);

        Assert.Contains("Pong!", context.TextResponses);
    }
}
