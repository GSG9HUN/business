using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Service.ReactionHandler;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.ReactionHandler;

public abstract class ReactionHandlerServiceTestBase
{
    protected ReactionHandlerServiceTestBase()
    {
        LoggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        LocalizationServiceMock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<object[]>())).Returns("test");
        LocalizationServiceMock.Setup(x => x.Get(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns("test");
        LocalizationServiceMock.Setup(x => x.Get(LocalizationKeys.ReactionHandlerRepeatOn, It.IsAny<object[]>()))
            .Returns("Repeat on");
        LocalizationServiceMock.Setup(x => x.Get(LocalizationKeys.ReactionHandlerRepeatOff, It.IsAny<object[]>()))
            .Returns("Repeat off");
        LocalizationServiceMock
            .Setup(x => x.Get(It.IsAny<ulong>(), LocalizationKeys.ReactionHandlerRepeatOn, It.IsAny<object[]>()))
            .Returns("Repeat on");
        LocalizationServiceMock
            .Setup(x => x.Get(It.IsAny<ulong>(), LocalizationKeys.ReactionHandlerRepeatOff, It.IsAny<object[]>()))
            .Returns("Repeat off");
    }

    protected Mock<ILavaLinkService> LavaLinkServiceMock { get; } = new();
    protected Mock<ILogger<ReactionHandlerService>> LoggerMock { get; } = new();
    protected Mock<ILocalizationService> LocalizationServiceMock { get; } = new();
    protected Mock<IProgressiveTimerService> ProgressiveTimerServiceMock { get; } = new();

    protected ReactionHandlerService CreateHandler(bool isTestMode = false)
    {
        return new ReactionHandlerService(
            LavaLinkServiceMock.Object,
            LoggerMock.Object,
            ProgressiveTimerServiceMock.Object,
            LocalizationServiceMock.Object,
            isTestMode);
    }

    protected static DiscordClient CreateDiscordClient()
    {
        return TestDiscordClientFactory.Create("test-token");
    }

    protected ReactionTarget CreateReactionTarget(ulong guildId = 123UL)
    {
        return new ReactionTarget(guildId);
    }

    protected void SetupSuccessfulPlaybackOperations()
    {
        LavaLinkServiceMock.Setup(x => x.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .Returns(Task.CompletedTask);
        LavaLinkServiceMock.Setup(x => x.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .Returns(Task.CompletedTask);
        LavaLinkServiceMock.Setup(x => x.SkipAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .Returns(Task.CompletedTask);
    }

    protected void ResetLogger()
    {
        LoggerMock.Reset();
        LoggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
    }

    protected void VerifyLog(LogLevel level, int eventId, string? messageContains = null, Exception? exception = null)
    {
        if (exception == null)
        {
            LoggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == level),
                    It.Is<EventId>(e => e.Id == eventId),
                    It.Is<It.IsAnyType>((v, _) =>
                        messageContains == null || v.ToString()!.Contains(messageContains)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once);
            return;
        }

        LoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.Is<EventId>(e => e.Id == eventId),
                It.Is<It.IsAnyType>((v, _) =>
                    messageContains == null || v.ToString()!.Contains(messageContains)),
                It.Is<Exception>(ex => ReferenceEquals(ex, exception)),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once);
    }

    protected sealed class ReactionTarget
    {
        public ReactionTarget(ulong guildId)
        {
            GuildMock.SetupGet(g => g.Id).Returns(guildId);
            ChannelMock.SetupGet(c => c.Guild).Returns(GuildMock.Object);
            MessageMock.SetupGet(m => m.Channel).Returns(ChannelMock.Object);
            MessageMock.Setup(x => x.RespondAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        }

        public Mock<IDiscordMessage> MessageMock { get; } = new();
        public Mock<IDiscordChannel> ChannelMock { get; } = new();
        public Mock<IDiscordGuild> GuildMock { get; } = new();
        public Mock<IDiscordMember> MemberMock { get; } = new();

        public IDiscordMessage Message => MessageMock.Object;
        public IDiscordMember Member => MemberMock.Object;
        public IDiscordChannel Channel => ChannelMock.Object;
    }
}
