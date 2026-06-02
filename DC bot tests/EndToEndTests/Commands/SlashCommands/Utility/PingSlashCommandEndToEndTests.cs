using DC_bot.Commands.SlashCommands.Utility;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.EndToEndTests.Commands.SlashCommands.Utility;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class PingSlashCommandEndToEndTests : SlashCommandPipelineEndToEndTestBase
{
    [Fact]
    public async Task PingSlashCommand_ShouldRunThroughTextCommandPipeline()
    {
        var context = CreateContext();
        var command = new PingSlashCommand(SlashCommandExecutor, Mock.Of<ISlashInteractionContextFactory>());

        await command.ExecuteAsync(context);

        Assert.Contains("Pong!", context.TextResponses);
    }
}
