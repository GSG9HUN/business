using DC_bot.Constants;
using DC_bot.Exceptions.Messaging;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Service;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service;

[Trait("Category", "Unit")]
public class ReactionHandlerTests
{
    [Fact]
    public void RegisterHandler_WithDiscordClient_SubscribesToEvents()
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var loggerMock = new Mock<ILogger<ReactionHandler>>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var progressiveTimerServiceMock = new Mock<IProgressiveTimerService>();

        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        localizationServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns("test");

        var discordConfig = new DiscordConfiguration
        {
            Token = "test-token",
            Intents = DiscordIntents.AllUnprivileged
        };
        var discordClient = new DiscordClient(discordConfig);

        var reactionHandler = new ReactionHandler(
            lavaLinkServiceMock.Object,
            loggerMock.Object,
            progressiveTimerServiceMock.Object,
            localizationServiceMock.Object
        );

        reactionHandler.RegisterHandler(discordClient);

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1202),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registered reaction handler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        reactionHandler.UnregisterHandler(discordClient);
    }

    [Fact]
    public void RegisterHandler_ThenUnregister_UnsubscribesFromEvents()
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var loggerMock = new Mock<ILogger<ReactionHandler>>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var progressiveTimerServiceMock = new Mock<IProgressiveTimerService>();

        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        localizationServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns("test");

        var discordConfig = new DiscordConfiguration
        {
            Token = "test-token",
            Intents = DiscordIntents.AllUnprivileged
        };
        var discordClient = new DiscordClient(discordConfig);

        var reactionHandler = new ReactionHandler(
            lavaLinkServiceMock.Object,
            loggerMock.Object,
            progressiveTimerServiceMock.Object,
            localizationServiceMock.Object
        );

        reactionHandler.RegisterHandler(discordClient);
        reactionHandler.UnregisterHandler(discordClient);

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1203),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unregistered reaction handler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void RegisterHandler_CalledTwice_LogsAlreadyRegisteredSecondTime()
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var loggerMock = new Mock<ILogger<ReactionHandler>>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var progressiveTimerServiceMock = new Mock<IProgressiveTimerService>();

        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        localizationServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns("test");

        var discordConfig = new DiscordConfiguration
        {
            Token = "test-token",
            Intents = DiscordIntents.AllUnprivileged
        };
        var discordClient = new DiscordClient(discordConfig);

        var reactionHandler = new ReactionHandler(
            lavaLinkServiceMock.Object,
            loggerMock.Object,
            progressiveTimerServiceMock.Object,
            localizationServiceMock.Object
        );

        reactionHandler.RegisterHandler(discordClient);
        reactionHandler.RegisterHandler(discordClient);

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1201),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ReactionHandler Service is already registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        reactionHandler.UnregisterHandler(discordClient);
    }

    [Fact]
    public void UnregisterHandler_WithoutPreviousRegister_LogsWarning()
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var loggerMock = new Mock<ILogger<ReactionHandler>>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var progressiveTimerServiceMock = new Mock<IProgressiveTimerService>();

        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        localizationServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns("test");

        var discordConfig = new DiscordConfiguration
        {
            Token = "test-token",
            Intents = DiscordIntents.AllUnprivileged
        };
        var discordClient = new DiscordClient(discordConfig);

        var reactionHandler = new ReactionHandler(
            lavaLinkServiceMock.Object,
            loggerMock.Object,
            progressiveTimerServiceMock.Object,
            localizationServiceMock.Object
        );

        reactionHandler.UnregisterHandler(discordClient);

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.Is<EventId>(e => e.Id == 1204),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Tried to unregister handlers, but it was not registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void RegisterUnregisterCycle_MaintainsConsistentState()
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var loggerMock = new Mock<ILogger<ReactionHandler>>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var progressiveTimerServiceMock = new Mock<IProgressiveTimerService>();

        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        localizationServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns("test");

        var discordConfig = new DiscordConfiguration
        {
            Token = "test-token",
            Intents = DiscordIntents.AllUnprivileged
        };
        var discordClient = new DiscordClient(discordConfig);

        var reactionHandler = new ReactionHandler(
            lavaLinkServiceMock.Object,
            loggerMock.Object,
            progressiveTimerServiceMock.Object,
            localizationServiceMock.Object
        );

        reactionHandler.RegisterHandler(discordClient);
        loggerMock.Invocations.Clear();

        reactionHandler.UnregisterHandler(discordClient);
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1203),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        loggerMock.Reset();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        reactionHandler.RegisterHandler(discordClient);
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1202),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        reactionHandler.UnregisterHandler(discordClient);
    }

    public static IEnumerable<object[]> AddedReactionCases()
    {
        yield return ["⏸️", "Pause"];
        yield return ["▶️", "Resume"];
        yield return ["⏭️", "Skip"];
        yield return ["🔁", "RepeatOn"];
    }

    public static IEnumerable<object[]> RemovedReactionCases()
    {
        yield return ["⏸️", "Resume"];
        yield return ["▶️", "Pause"];
        yield return ["⏭️", "Skip"];
        yield return ["🔁", "RepeatOff"];
    }

    [Theory]
    [MemberData(nameof(AddedReactionCases))]
    public async Task HandleReactionAddedAsync_WhenSupportedEmoji_ExecutesExpectedAction(string emoji, string expectedAction)
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var loggerMock = new Mock<ILogger<ReactionHandler>>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var progressiveTimerServiceMock = new Mock<IProgressiveTimerService>();
        var messageMock = new Mock<IDiscordMessage>();
        var memberMock = new Mock<IDiscordMember>();

        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        localizationServiceMock.Setup(x => x.Get(LocalizationKeys.ReactionHandlerRepeatOn)).Returns("Repeat on");
        localizationServiceMock.Setup(x => x.Get(LocalizationKeys.ReactionHandlerRepeatOff)).Returns("Repeat off");
        lavaLinkServiceMock.Setup(x => x.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .Returns(Task.CompletedTask);
        lavaLinkServiceMock.Setup(x => x.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .Returns(Task.CompletedTask);
        lavaLinkServiceMock.Setup(x => x.SkipAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .Returns(Task.CompletedTask);
        messageMock.Setup(x => x.RespondAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        var reactionHandler = new ReactionHandler(
            lavaLinkServiceMock.Object,
            loggerMock.Object,
            progressiveTimerServiceMock.Object,
            localizationServiceMock.Object
        );

        await reactionHandler.HandleReactionAddedAsync(emoji, messageMock.Object, memberMock.Object);

        lavaLinkServiceMock.Verify(x => x.PauseAsync(messageMock.Object, memberMock.Object),
            expectedAction == "Pause" ? Times.Once() : Times.Never());
        lavaLinkServiceMock.Verify(x => x.ResumeAsync(messageMock.Object, memberMock.Object),
            expectedAction == "Resume" ? Times.Once() : Times.Never());
        lavaLinkServiceMock.Verify(x => x.SkipAsync(messageMock.Object, memberMock.Object),
            expectedAction == "Skip" ? Times.Once() : Times.Never());
        messageMock.Verify(x => x.RespondAsync("Repeat on"),
            expectedAction == "RepeatOn" ? Times.Once() : Times.Never());
    }

    [Theory]
    [MemberData(nameof(RemovedReactionCases))]
    public async Task HandleReactionRemovedAsync_WhenSupportedEmoji_ExecutesExpectedAction(string emoji, string expectedAction)
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var loggerMock = new Mock<ILogger<ReactionHandler>>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var progressiveTimerServiceMock = new Mock<IProgressiveTimerService>();
        var messageMock = new Mock<IDiscordMessage>();
        var memberMock = new Mock<IDiscordMember>();

        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        localizationServiceMock.Setup(x => x.Get(LocalizationKeys.ReactionHandlerRepeatOn)).Returns("Repeat on");
        localizationServiceMock.Setup(x => x.Get(LocalizationKeys.ReactionHandlerRepeatOff)).Returns("Repeat off");
        lavaLinkServiceMock.Setup(x => x.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .Returns(Task.CompletedTask);
        lavaLinkServiceMock.Setup(x => x.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .Returns(Task.CompletedTask);
        lavaLinkServiceMock.Setup(x => x.SkipAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .Returns(Task.CompletedTask);
        messageMock.Setup(x => x.RespondAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        var reactionHandler = new ReactionHandler(
            lavaLinkServiceMock.Object,
            loggerMock.Object,
            progressiveTimerServiceMock.Object,
            localizationServiceMock.Object
        );

        await reactionHandler.HandleReactionRemovedAsync(emoji, messageMock.Object, memberMock.Object);

        lavaLinkServiceMock.Verify(x => x.PauseAsync(messageMock.Object, memberMock.Object),
            expectedAction == "Pause" ? Times.Once() : Times.Never());
        lavaLinkServiceMock.Verify(x => x.ResumeAsync(messageMock.Object, memberMock.Object),
            expectedAction == "Resume" ? Times.Once() : Times.Never());
        lavaLinkServiceMock.Verify(x => x.SkipAsync(messageMock.Object, memberMock.Object),
            expectedAction == "Skip" ? Times.Once() : Times.Never());
        messageMock.Verify(x => x.RespondAsync("Repeat off"),
            expectedAction == "RepeatOff" ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task ExecuteOnReactionAddedAsync_WhenBotExceptionThrown_LogsOperationFailedWithEventId1208()
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var loggerMock = new Mock<ILogger<ReactionHandler>>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var progressiveTimerServiceMock = new Mock<IProgressiveTimerService>();
        var messageMock = new Mock<IDiscordMessage>();
        var memberMock = new Mock<IDiscordMember>();

        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var botException = new LavalinkOperationException("PauseAsync", "player not found");
        lavaLinkServiceMock
            .Setup(x => x.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .ThrowsAsync(botException);

        var reactionHandler = new ReactionHandler(
            lavaLinkServiceMock.Object,
            loggerMock.Object,
            progressiveTimerServiceMock.Object,
            localizationServiceMock.Object
        );

        await reactionHandler.ExecuteOnReactionAddedAsync("⏸️", messageMock.Object, memberMock.Object);

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.Is<EventId>(e => e.Id == 1208),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("OnReactionAdded")),
                It.Is<Exception>(ex => ex == botException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteOnReactionAddedAsync_WhenGeneralExceptionThrown_LogsOperationFailedWithEventId1208()
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var loggerMock = new Mock<ILogger<ReactionHandler>>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var progressiveTimerServiceMock = new Mock<IProgressiveTimerService>();
        var messageMock = new Mock<IDiscordMessage>();
        var memberMock = new Mock<IDiscordMember>();

        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var generalException = new InvalidOperationException("unexpected failure");
        lavaLinkServiceMock
            .Setup(x => x.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .ThrowsAsync(generalException);

        var reactionHandler = new ReactionHandler(
            lavaLinkServiceMock.Object,
            loggerMock.Object,
            progressiveTimerServiceMock.Object,
            localizationServiceMock.Object
        );

        await reactionHandler.ExecuteOnReactionAddedAsync("▶️", messageMock.Object, memberMock.Object);

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.Is<EventId>(e => e.Id == 1208),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("OnReactionAdded")),
                It.Is<Exception>(ex => ex == generalException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteOnReactionRemovedAsync_WhenBotExceptionThrown_LogsOperationFailedWithEventId1208()
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var loggerMock = new Mock<ILogger<ReactionHandler>>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var progressiveTimerServiceMock = new Mock<IProgressiveTimerService>();
        var messageMock = new Mock<IDiscordMessage>();
        var memberMock = new Mock<IDiscordMember>();

        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var botException = new LavalinkOperationException("ResumeAsync", "player not found");
        lavaLinkServiceMock
            .Setup(x => x.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .ThrowsAsync(botException);

        var reactionHandler = new ReactionHandler(
            lavaLinkServiceMock.Object,
            loggerMock.Object,
            progressiveTimerServiceMock.Object,
            localizationServiceMock.Object
        );

        await reactionHandler.ExecuteOnReactionRemovedAsync("⏸️", messageMock.Object, memberMock.Object);

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.Is<EventId>(e => e.Id == 1208),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("OnReactionRemoved")),
                It.Is<Exception>(ex => ex == botException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteOnReactionRemovedAsync_WhenGeneralExceptionThrown_LogsOperationFailedWithEventId1208()
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var loggerMock = new Mock<ILogger<ReactionHandler>>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var progressiveTimerServiceMock = new Mock<IProgressiveTimerService>();
        var messageMock = new Mock<IDiscordMessage>();
        var memberMock = new Mock<IDiscordMember>();

        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var generalException = new InvalidOperationException("unexpected failure");
        lavaLinkServiceMock
            .Setup(x => x.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()))
            .ThrowsAsync(generalException);

        var reactionHandler = new ReactionHandler(
            lavaLinkServiceMock.Object,
            loggerMock.Object,
            progressiveTimerServiceMock.Object,
            localizationServiceMock.Object
        );

        await reactionHandler.ExecuteOnReactionRemovedAsync("▶️", messageMock.Object, memberMock.Object);

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.Is<EventId>(e => e.Id == 1208),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("OnReactionRemoved")),
                It.Is<Exception>(ex => ex == generalException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task SendReactionControlMessage_WhenSendFails_LogsEventId1209AndThrowsMessageSendException()
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var loggerMock = new Mock<ILogger<ReactionHandler>>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var progressiveTimerServiceMock = new Mock<IProgressiveTimerService>();
        var channelMock = new Mock<IDiscordChannel>();

        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        localizationServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns("test");

        var sendException = new InvalidOperationException("Discord API failure");
        channelMock.Setup(x => x.ToDiscordChannel()).Throws(sendException);

        var discordConfig = new DiscordConfiguration { Token = "test-token", Intents = DiscordIntents.AllUnprivileged };
        var discordClient = new DiscordClient(discordConfig);

        var reactionHandler = new ReactionHandler(
            lavaLinkServiceMock.Object,
            loggerMock.Object,
            progressiveTimerServiceMock.Object,
            localizationServiceMock.Object
        );
        reactionHandler.RegisterHandler(discordClient);

        var exception = await Assert.ThrowsAsync<MessageSendException>(() => lavaLinkServiceMock.RaiseAsync(
            x => x.TrackStarted += null!,
            channelMock.Object,
            discordClient,
            new DiscordEmbedBuilder().WithTitle("test").Build()));

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.Is<EventId>(e => e.Id == 1209),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("SendReactionControlMessage")),
                It.Is<Exception>(ex => ex == sendException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        Assert.Same(sendException, exception.InnerException);
        reactionHandler.UnregisterHandler(discordClient);
    }
}

