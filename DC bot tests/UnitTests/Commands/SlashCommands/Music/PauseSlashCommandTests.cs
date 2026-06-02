using DC_bot.Commands.SlashCommands.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.SlashCommands.Music;

[Trait("Category", "Unit")]
public class PauseSlashCommandTests : SlashCommandTestBase
{
    [Fact]
    public async Task Pause_ShouldCreateInteractionContextAndDelegateToExecutor()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new PauseSlashCommand(executor.Object, contextFactory.Object);

        await command.Pause(dsharpContext);

        contextFactory.Verify(x => x.Create(dsharpContext), Times.Once);
        VerifyRequest(
            executor,
            "pause",
            slashContext.Object,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallPauseServiceAfterDeferring()
    {
        var voiceChannel = CreateVoiceChannel();
        var member = CreateMember("SlashUser", "<@123>", voiceChannel);
        var context = CreateContext(member: member);

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "pause",
            context,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        Assert.True(context.IsDeferred);
        Assert.Contains("Request accepted.", context.TextResponses);
        LavaLinkServiceMock.Verify(
            service => service.PauseAsync(It.IsAny<IDiscordMessage>(), member),
            Times.Once);
    }
}
