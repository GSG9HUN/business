using DC_bot.Commands.SlashCommands.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.SlashCommands.Music;

[Trait("Category", "Unit")]
public class JoinSlashCommandTests : SlashCommandTestBase
{
    [Fact]
    public async Task Join_ShouldCreateInteractionContextAndDelegateToExecutor()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new JoinSlashCommand(executor.Object, contextFactory.Object);

        await command.Join(dsharpContext);

        contextFactory.Verify(x => x.Create(dsharpContext), Times.Once);
        VerifyRequest(
            executor,
            "join",
            slashContext.Object,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldStartPlayingQueueAfterDeferring()
    {
        var voiceChannel = CreateVoiceChannel();
        var member = CreateMember("SlashUser", "<@123>", voiceChannel);
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
