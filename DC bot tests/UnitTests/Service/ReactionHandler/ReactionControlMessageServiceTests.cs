using DC_bot.Constants;
using DC_bot.Exceptions.Messaging;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Service.ReactionHandler;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.ReactionHandler;

[Trait("Category", "Unit")]
public class ReactionControlMessageServiceTests
{
    [Fact]
    public async Task SendAsync_WhenDiscordChannelCannotBeResolved_ThrowsMessageSendException()
    {
        var context = CreateFailingContext();

        var exception = await Assert.ThrowsAsync<MessageSendException>(
            () => context.Service.SendAsync(
                context.Channel.Object,
                TestDiscordClientFactory.Create("test-token"),
                new DiscordEmbedBuilder().WithTitle("track").Build()));

        Assert.Equal("SendReactionControlMessage", exception.Operation);
        Assert.Same(context.SendException, exception.InnerException);
    }

    [Fact]
    public async Task SendAsync_WhenDiscordChannelCannotBeResolved_LogsEventId1209()
    {
        var context = CreateFailingContext();

        await Assert.ThrowsAsync<MessageSendException>(
            () => context.Service.SendAsync(
                context.Channel.Object,
                TestDiscordClientFactory.Create("test-token"),
                new DiscordEmbedBuilder().WithTitle("track").Build()));

        context.Logger.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(level => level == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 1209),
                It.Is<It.IsAnyType>((value, _) => value.ToString()!.Contains("SendReactionControlMessage")),
                It.Is<Exception>(exception => exception == context.SendException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_WhenDiscordChannelCannotBeResolved_DoesNotStartProgressiveTimer()
    {
        var context = CreateFailingContext();

        await Assert.ThrowsAsync<MessageSendException>(
            () => context.Service.SendAsync(
                context.Channel.Object,
                TestDiscordClientFactory.Create("test-token"),
                new DiscordEmbedBuilder().WithTitle("track").Build()));

        context.ProgressiveTimer.Verify(
            timer => timer.StartAsync(It.IsAny<IDiscordMessage>(), It.IsAny<ulong>()),
            Times.Never);
    }

    [Fact]
    public async Task SendAsync_BuildsLocalizedControlMessageBeforeSending()
    {
        var context = CreateFailingContext();

        await Assert.ThrowsAsync<MessageSendException>(
            () => context.Service.SendAsync(
                context.Channel.Object,
                TestDiscordClientFactory.Create("test-token"),
                new DiscordEmbedBuilder().WithTitle("track").Build()));

        context.Localization.Verify(
            service => service.Get(123UL, LocalizationKeys.MusicControl),
            Times.Once);
        context.Localization.Verify(
            service => service.Get(123UL, LocalizationKeys.PauseReaction),
            Times.Once);
        context.Localization.Verify(
            service => service.Get(123UL, LocalizationKeys.ResumeReaction),
            Times.Once);
        context.Localization.Verify(
            service => service.Get(123UL, LocalizationKeys.SkipReaction),
            Times.Once);
        context.Localization.Verify(
            service => service.Get(123UL, LocalizationKeys.RepeatReaction),
            Times.Once);
    }

    private static ControlMessageTestContext CreateFailingContext()
    {
        var progressiveTimerServiceMock = new Mock<IProgressiveTimerService>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var loggerMock = new Mock<ILogger<ReactionControlMessageService>>();
        var channelMock = new Mock<IDiscordChannel>();
        var guildMock = new Mock<IDiscordGuild>();
        var sendException = new InvalidOperationException("Discord channel unavailable");

        loggerMock.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        guildMock.SetupGet(guild => guild.Id).Returns(123UL);
        channelMock.SetupGet(channel => channel.Guild).Returns(guildMock.Object);
        channelMock.Setup(channel => channel.ToDiscordChannel()).Throws(sendException);

        localizationServiceMock
            .Setup(service => service.Get(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<ulong, string, object[]>((_, key, _) => key);

        return new ControlMessageTestContext(
            new ReactionControlMessageService(
                progressiveTimerServiceMock.Object,
                localizationServiceMock.Object,
                loggerMock.Object),
            progressiveTimerServiceMock,
            localizationServiceMock,
            loggerMock,
            channelMock,
            sendException);
    }

    private sealed record ControlMessageTestContext(
        ReactionControlMessageService Service,
        Mock<IProgressiveTimerService> ProgressiveTimer,
        Mock<ILocalizationService> Localization,
        Mock<ILogger<ReactionControlMessageService>> Logger,
        Mock<IDiscordChannel> Channel,
        Exception SendException);
}
