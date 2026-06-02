using DC_bot.Commands.SlashCommands.Utility;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.EndToEndTests.Commands.SlashCommands.Utility;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class LanguageSlashCommandEndToEndTests : SlashCommandPipelineEndToEndTestBase
{
    [Fact]
    public async Task LanguageSlashCommand_ShouldRunThroughTextCommandPipeline()
    {
        var context = CreateContext();
        var command = new LanguageSlashCommand(SlashCommandExecutor, Mock.Of<ISlashInteractionContextFactory>());

        await command.ExecuteAsync(context, SlashLanguage.Hu);

        Assert.True(context.IsDeferred);
        Assert.Contains("A nyelv sikeresen megvaltozott.", context.TextResponses);
        LocalizationServiceMock.Verify(service => service.SaveLanguage(123UL, "hu"), Times.Once);
    }
}
