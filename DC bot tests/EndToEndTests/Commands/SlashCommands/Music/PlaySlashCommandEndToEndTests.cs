using DC_bot.Commands.SlashCommands.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using Lavalink4NET.Rest.Entities.Tracks;
using Moq;

namespace DC_bot_tests.EndToEndTests.Commands.SlashCommands.Music;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class PlaySlashCommandEndToEndTests : SlashCommandPipelineEndToEndTestBase
{
    [Fact]
    public async Task PlaySlashCommand_ShouldRunThroughTextCommandPipeline()
    {
        var voiceChannel = CreateVoiceChannel();
        var context = CreateContext(member: CreateMember("SlashUser", "<@123>", voiceChannel));
        var command = new PlaySlashCommand(SlashCommandExecutor, Mock.Of<ISlashInteractionContextFactory>());

        await command.ExecuteAsync(context, "madeon imperium");

        LavaLinkServiceMock.Verify(
            service => service.PlayAsyncQuery(
                voiceChannel,
                "madeon imperium",
                It.IsAny<IDiscordMessage>(),
                TrackSearchMode.YouTube),
            Times.Once);
        Assert.Contains("Request accepted.", context.TextResponses);
    }
}
