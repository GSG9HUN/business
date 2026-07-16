using DC_bot.Exceptions.Music;
using DC_bot.Interface.Discord;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.ReactionHandler;

[Trait("Category", "Unit")]
public class ReactionHandlerServiceExceptionLoggingTests : ReactionHandlerServiceTestBase
{
    [Fact]
    public async Task ExecuteOnReactionAddedAsync_WhenBotExceptionThrown_LogsOperationFailedWithEventId1208()
    {
        var target = CreateReactionTarget();
        var botException = new LavalinkOperationException("PauseAsync", "player not found");
        LavaLinkServiceMock
            .Setup(x => x.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .ThrowsAsync(botException);
        var reactionHandler = CreateHandler();

        await reactionHandler.ExecuteOnReactionAddedAsync("⏸️", target.Message, target.Member);

        VerifyLog(LogLevel.Error, 1208, "OnReactionAdded", botException);
    }

    [Fact]
    public async Task ExecuteOnReactionAddedAsync_WhenGeneralExceptionThrown_LogsOperationFailedWithEventId1208()
    {
        var target = CreateReactionTarget();
        var generalException = new InvalidOperationException("unexpected failure");
        LavaLinkServiceMock
            .Setup(x => x.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .ThrowsAsync(generalException);
        var reactionHandler = CreateHandler();

        await reactionHandler.ExecuteOnReactionAddedAsync("▶️", target.Message, target.Member);

        VerifyLog(LogLevel.Error, 1208, "OnReactionAdded", generalException);
    }

    [Fact]
    public async Task ExecuteOnReactionRemovedAsync_WhenBotExceptionThrown_LogsOperationFailedWithEventId1208()
    {
        var target = CreateReactionTarget();
        var botException = new LavalinkOperationException("ResumeAsync", "player not found");
        LavaLinkServiceMock
            .Setup(x => x.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .ThrowsAsync(botException);
        var reactionHandler = CreateHandler();

        await reactionHandler.ExecuteOnReactionRemovedAsync("⏸️", target.Message, target.Member);

        VerifyLog(LogLevel.Error, 1208, "OnReactionRemoved", botException);
    }

    [Fact]
    public async Task ExecuteOnReactionRemovedAsync_WhenGeneralExceptionThrown_LogsOperationFailedWithEventId1208()
    {
        var target = CreateReactionTarget();
        var generalException = new InvalidOperationException("unexpected failure");
        LavaLinkServiceMock
            .Setup(x => x.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .ThrowsAsync(generalException);
        var reactionHandler = CreateHandler();

        await reactionHandler.ExecuteOnReactionRemovedAsync("▶️", target.Message, target.Member);

        VerifyLog(LogLevel.Error, 1208, "OnReactionRemoved", generalException);
    }
}
