using DC_bot.Commands.SlashCommands.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.EndToEndTests.Commands.SlashCommands.Music;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class ResumeSlashCommandEndToEndTests : SlashCommandPipelineEndToEndTestBase
{
    [Fact]
    public async Task ResumeSlashCommand_ShouldRunThroughTextCommandPipeline()
    {
        var member = CreateMember("SlashUser", "<@123>", CreateVoiceChannel());
        var context = CreateContext(member: member);
        var command = new ResumeSlashCommand(SlashCommandExecutor, Mock.Of<ISlashInteractionContextFactory>());

        await command.ExecuteAsync(context);

        Assert.True(context.IsDeferred);
        Assert.Contains("Request accepted.", context.TextResponses);
        LavaLinkServiceMock.Verify(
            service => service.ResumeAsync(It.IsAny<IDiscordMessage>(), member),
            Times.Once);
    }
}
