using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Service;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service;

public class ReactionHandlerTests
{
    [Fact]
    public void RegisterHandler_WithDiscordClient_SubscribesToEvents()
    {
        // Arrange
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

        // Act
        reactionHandler.RegisterHandler(discordClient);

        // Assert
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
        // Arrange
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

        // Act
        reactionHandler.RegisterHandler(discordClient);
        reactionHandler.UnregisterHandler(discordClient);

        // Assert
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
        // Arrange
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

        // Act
        reactionHandler.RegisterHandler(discordClient);
        reactionHandler.RegisterHandler(discordClient);

        // Assert
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
        // Arrange
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

        // Act
        reactionHandler.UnregisterHandler(discordClient);

        // Assert
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
        // Arrange
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

        // Act & Assert
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
}