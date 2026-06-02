using DC_bot.Commands.SlashCommands.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.SlashCommands.Music;

[Trait("Category", "Unit")]
public class LeaveSlashCommandTests : SlashCommandTestBase
{
    [Fact]
    public async Task Leave_ShouldCreateInteractionContextAndDelegateToExecutor()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new LeaveSlashCommand(executor.Object, contextFactory.Object);

        await command.Leave(dsharpContext);

        contextFactory.Verify(x => x.Create(dsharpContext), Times.Once);
        VerifyRequest(
            executor,
            "leave",
            slashContext.Object,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallLeaveServiceAfterDeferring()
    {
        var voiceChannel = CreateVoiceChannel();
        var member = CreateMember("SlashUser", "<@123>", voiceChannel);
        var context = CreateContext(member: member);

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "leave",
            context,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        Assert.True(context.IsDeferred);
        Assert.Contains("Request accepted.", context.TextResponses);
        LavaLinkServiceMock.Verify(
            service => service.LeaveVoiceChannel(It.IsAny<IDiscordMessage>(), member),
            Times.Once);
    }
}
