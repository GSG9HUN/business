using DC_bot.Commands.SlashCommands.Utility;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.EndToEndTests.Commands.SlashCommands.Utility;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class TagSlashCommandEndToEndTests : SlashCommandPipelineEndToEndTestBase
{
    [Fact]
    public async Task TagSlashCommand_ShouldRunThroughTextCommandPipeline()
    {
        var taggedMember = CreateMember("TargetUser", "<@999>");
        var context = CreateContext(allMembers: [taggedMember]);
        var command = new TagSlashCommand(SlashCommandExecutor, Mock.Of<ISlashInteractionContextFactory>());

        await command.ExecuteAsync(context, taggedMember);

        Assert.True(context.IsDeferred);
        Assert.Contains("Tagged: <@999>", context.TextResponses);
    }
}
