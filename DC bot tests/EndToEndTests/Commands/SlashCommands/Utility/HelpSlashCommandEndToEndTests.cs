using DC_bot.Commands.SlashCommands.Utility;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.EndToEndTests.Commands.SlashCommands.Utility;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class HelpSlashCommandEndToEndTests : SlashCommandPipelineEndToEndTestBase
{
    [Fact]
    public async Task HelpSlashCommand_ShouldRunThroughTextCommandPipeline()
    {
        var context = CreateContext();
        var command = new HelpSlashCommand(SlashCommandExecutor, Mock.Of<ISlashInteractionContextFactory>());

        await command.ExecuteAsync(context);

        var response = Assert.Single(context.TextResponses);
        Assert.Contains("Available commands:", response);
        Assert.Contains("ping : Replies with Pong!", response);
    }
}
