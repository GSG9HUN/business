using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Service.ReactionHandler;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service.ReactionHandler;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class ReactionHandlerDispatchIntegrationTests
{
    public static IEnumerable<object[]> AddedReactionCases()
    {
        yield return [ReactionControlEmojis.PauseEmojiName, "Pause"];
        yield return [ReactionControlEmojis.ResumeEmojiName, "Resume"];
        yield return [ReactionControlEmojis.SkipEmojiName, "Skip"];
    }

    public static IEnumerable<object[]> RemovedReactionCases()
    {
        yield return [ReactionControlEmojis.PauseEmojiName, "Resume"];
        yield return [ReactionControlEmojis.ResumeEmojiName, "Pause"];
        yield return [ReactionControlEmojis.SkipEmojiName, "Skip"];
    }

    [Theory]
    [MemberData(nameof(AddedReactionCases))]
    public async Task ExecuteOnReactionAddedAsync_UsesDefaultDispatcher(string emojiName, string expectedAction)
    {
        var context = CreateHandlerContext();

        await context.Service.ExecuteOnReactionAddedAsync(emojiName, context.Message.Object, context.Member.Object);

        VerifyPlaybackAction(context, expectedAction);
    }

    [Theory]
    [MemberData(nameof(RemovedReactionCases))]
    public async Task ExecuteOnReactionRemovedAsync_UsesDefaultDispatcher(string emojiName, string expectedAction)
    {
        var context = CreateHandlerContext();

        await context.Service.ExecuteOnReactionRemovedAsync(emojiName, context.Message.Object, context.Member.Object);

        VerifyPlaybackAction(context, expectedAction);
    }

    private static HandlerIntegrationContext CreateHandlerContext()
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var progressiveTimerServiceMock = new Mock<IProgressiveTimerService>();
        var loggerMock = new Mock<ILogger<ReactionHandlerService>>();
        var messageMock = new Mock<IDiscordMessage>();
        var memberMock = new Mock<IDiscordMember>();

        loggerMock.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        messageMock.SetupGet(message => message.Id).Returns(654UL);
        memberMock.SetupGet(member => member.Id).Returns(789UL);

        lavaLinkServiceMock
            .Setup(service => service.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .Returns(Task.CompletedTask);
        lavaLinkServiceMock
            .Setup(service => service.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .Returns(Task.CompletedTask);
        lavaLinkServiceMock
            .Setup(service => service.SkipAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .Returns(Task.CompletedTask);

        var service = new ReactionHandlerService(
            lavaLinkServiceMock.Object,
            loggerMock.Object,
            progressiveTimerServiceMock.Object,
            localizationServiceMock.Object);

        return new HandlerIntegrationContext(service, lavaLinkServiceMock, messageMock, memberMock);
    }

    private static void VerifyPlaybackAction(HandlerIntegrationContext context, string expectedAction)
    {
        context.LavaLink.Verify(
            service => service.PauseAsync(context.Message.Object, context.Member.Object),
            expectedAction == "Pause" ? Times.Once() : Times.Never());
        context.LavaLink.Verify(
            service => service.ResumeAsync(context.Message.Object, context.Member.Object),
            expectedAction == "Resume" ? Times.Once() : Times.Never());
        context.LavaLink.Verify(
            service => service.SkipAsync(context.Message.Object, context.Member.Object),
            expectedAction == "Skip" ? Times.Once() : Times.Never());
    }

    private sealed record HandlerIntegrationContext(
        ReactionHandlerService Service,
        Mock<ILavaLinkService> LavaLink,
        Mock<IDiscordMessage> Message,
        Mock<IDiscordMember> Member);
}
