using DC_bot.Exceptions.Messaging;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.ReactionHandler;

[Trait("Category", "Unit")]
public class ReactionHandlerServiceControlMessageTests : ReactionHandlerServiceTestBase
{
    [Fact]
    public async Task SendReactionControlMessage_WhenSendFails_LogsEventId1209AndThrowsMessageSendException()
    {
        var target = CreateReactionTarget();
        var sendException = new InvalidOperationException("Discord API failure");
        target.ChannelMock.Setup(x => x.ToDiscordChannel()).Throws(sendException);

        var discordClient = CreateDiscordClient();
        var reactionHandler = CreateHandler();
        reactionHandler.RegisterHandler(discordClient);

        var exception = await Assert.ThrowsAsync<MessageSendException>(() => LavaLinkServiceMock.RaiseAsync(
            x => x.TrackStarted += null!,
            target.Channel,
            new DiscordEmbedBuilder().WithTitle("test").Build()));

        VerifyLog(LogLevel.Error, 1209, "SendReactionControlMessage", sendException);

        Assert.Same(sendException, exception.InnerException);
        reactionHandler.UnregisterHandler(discordClient);
    }
}
