using DC_bot.Commands.SlashCommands.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.EndToEndTests.Commands.SlashCommands.Music;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class JoinSlashCommandEndToEndTests : SlashCommandPipelineEndToEndTestBase
{
    [Fact]
    public async Task JoinSlashCommand_ShouldRunThroughTextCommandPipeline()
    {
        var member = CreateMember("SlashUser", "<@123>", CreateVoiceChannel());
        var context = CreateContext(member: member);
        var command = new JoinSlashCommand(SlashCommandExecutor, Mock.Of<ISlashInteractionContextFactory>());

        await command.ExecuteAsync(context);

        Assert.True(context.IsDeferred);
        Assert.Contains("Request accepted.", context.TextResponses);
        LavaLinkServiceMock.Verify(
            service => service.StartPlayingQueue(
                It.IsAny<IDiscordMessage>(),
                context.Channel,
                member),
            Times.Once);
    }
}
