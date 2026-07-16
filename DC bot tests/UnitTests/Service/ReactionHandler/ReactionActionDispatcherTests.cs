using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Service.ReactionHandler;
using Moq;

namespace DC_bot_tests.UnitTests.Service.ReactionHandler;

[Trait("Category", "Unit")]
public class ReactionActionDispatcherTests
{
    public static IEnumerable<object[]> AddedReactionCases()
    {
        yield return [ReactionControlEmojis.PauseEmojiName, "Pause"];
        yield return [ReactionControlEmojis.ResumeEmoji, "Resume"];
        yield return [ReactionControlEmojis.SkipEmojiName, "Skip"];
        yield return [ReactionControlEmojis.RepeatEmoji, "RepeatOn"];
    }

    public static IEnumerable<object[]> RemovedReactionCases()
    {
        yield return [ReactionControlEmojis.PauseEmoji, "Resume"];
        yield return [ReactionControlEmojis.ResumeEmojiName, "Pause"];
        yield return [ReactionControlEmojis.SkipEmoji, "Skip"];
        yield return [ReactionControlEmojis.RepeatEmojiName, "RepeatOff"];
    }

    [Theory]
    [MemberData(nameof(AddedReactionCases))]
    public async Task DispatchAddedAsync_WhenSupportedEmoji_RoutesExpectedAction(string emoji, string expectedAction)
    {
        var context = CreateContext();

        await context.Dispatcher.DispatchAddedAsync(emoji, context.Message.Object, context.Member.Object);

        VerifyPlaybackAction(context, expectedAction);
        context.Message.Verify(
            message => message.RespondAsync("Repeat on"),
            expectedAction == "RepeatOn" ? Times.Once() : Times.Never());
        context.Localization.Verify(
            service => service.Get(123UL, LocalizationKeys.ReactionHandlerRepeatOn),
            expectedAction == "RepeatOn" ? Times.Once() : Times.Never());
    }

    [Theory]
    [MemberData(nameof(RemovedReactionCases))]
    public async Task DispatchRemovedAsync_WhenSupportedEmoji_RoutesExpectedAction(string emoji, string expectedAction)
    {
        var context = CreateContext();

        await context.Dispatcher.DispatchRemovedAsync(emoji, context.Message.Object, context.Member.Object);

        VerifyPlaybackAction(context, expectedAction);
        context.Message.Verify(
            message => message.RespondAsync("Repeat off"),
            expectedAction == "RepeatOff" ? Times.Once() : Times.Never());
        context.Localization.Verify(
            service => service.Get(123UL, LocalizationKeys.ReactionHandlerRepeatOff),
            expectedAction == "RepeatOff" ? Times.Once() : Times.Never());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DispatchAsync_WhenEmojiIsUnsupported_DoesNotCallPlaybackOrRespond(bool isAddedReaction)
    {
        var context = CreateContext();

        if (isAddedReaction)
        {
            await context.Dispatcher.DispatchAddedAsync(":unsupported:", context.Message.Object, context.Member.Object);
        }
        else
        {
            await context.Dispatcher.DispatchRemovedAsync(":unsupported:", context.Message.Object, context.Member.Object);
        }

        context.LavaLink.Verify(
            service => service.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()),
            Times.Never);
        context.LavaLink.Verify(
            service => service.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()),
            Times.Never);
        context.LavaLink.Verify(
            service => service.SkipAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()),
            Times.Never);
        context.Message.Verify(message => message.RespondAsync(It.IsAny<string>()), Times.Never);
        context.Localization.Verify(
            service => service.Get(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<object[]>()),
            Times.Never);
    }

    private static DispatcherTestContext CreateContext()
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var messageMock = new Mock<IDiscordMessage>();
        var channelMock = new Mock<IDiscordChannel>();
        var guildMock = new Mock<IDiscordGuild>();
        var memberMock = new Mock<IDiscordMember>();

        guildMock.SetupGet(guild => guild.Id).Returns(123UL);
        channelMock.SetupGet(channel => channel.Guild).Returns(guildMock.Object);
        messageMock.SetupGet(message => message.Channel).Returns(channelMock.Object);
        messageMock.Setup(message => message.RespondAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        lavaLinkServiceMock
            .Setup(service => service.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .Returns(Task.CompletedTask);
        lavaLinkServiceMock
            .Setup(service => service.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .Returns(Task.CompletedTask);
        lavaLinkServiceMock
            .Setup(service => service.SkipAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .Returns(Task.CompletedTask);

        localizationServiceMock
            .Setup(service => service.Get(123UL, LocalizationKeys.ReactionHandlerRepeatOn))
            .Returns("Repeat on");
        localizationServiceMock
            .Setup(service => service.Get(123UL, LocalizationKeys.ReactionHandlerRepeatOff))
            .Returns("Repeat off");

        return new DispatcherTestContext(
            new ReactionActionDispatcher(lavaLinkServiceMock.Object, localizationServiceMock.Object),
            lavaLinkServiceMock,
            localizationServiceMock,
            messageMock,
            memberMock);
    }

    private static void VerifyPlaybackAction(DispatcherTestContext context, string expectedAction)
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

    private sealed record DispatcherTestContext(
        ReactionActionDispatcher Dispatcher,
        Mock<ILavaLinkService> LavaLink,
        Mock<ILocalizationService> Localization,
        Mock<IDiscordMessage> Message,
        Mock<IDiscordMember> Member);
}
