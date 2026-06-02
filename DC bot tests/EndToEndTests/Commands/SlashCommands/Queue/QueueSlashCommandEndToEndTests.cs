using DC_bot.Commands.SlashCommands.Queue;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.EndToEndTests.Commands.SlashCommands.Queue;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class QueueSlashCommandEndToEndTests : SlashCommandPipelineEndToEndTestBase
{
    [Fact]
    public async Task QueueSlashCommand_ShouldRunThroughTextCommandPipeline()
    {
        var track = CreateTrack("Track A", "Artist A");
        MusicQueueServiceMock
            .Setup(service => service.ViewQueue(123UL))
            .ReturnsAsync([track]);
        var context = CreateContext();
        var command = new QueueSlashCommand(SlashCommandExecutor, Mock.Of<ISlashInteractionContextFactory>());

        await command.ExecuteAsync(context);

        Assert.True(context.IsDeferred);
        var embed = Assert.Single(context.EmbedResponses);
        Assert.Equal("Playlist", embed.Title);
        Assert.NotNull(embed.Fields);
        Assert.Contains(embed.Fields, field => field.Name == "Track A");
    }
}
