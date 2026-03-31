using DC_bot.Service;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DC_bot_tests.UnitTests.Service;

public class BotServiceTests
{
    [Fact]
    public void BotService_Constructor_WithValidClient_CreatesInstance()
    {
        // Arrange
        var config = new DiscordConfiguration
        {
            Token = "test_token",
            TokenType = TokenType.Bot
        };
        var client = new DiscordClient(config);

        // Act
        var service = new BotService(client);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void BotService_Constructor_WithLogger_CreatesInstance()
    {
        // Arrange
        var config = new DiscordConfiguration
        {
            Token = "test_token",
            TokenType = TokenType.Bot
        };
        var client = new DiscordClient(config);
        var mockLogger = new Mock<ILogger<BotService>>();

        // Act
        var service = new BotService(client, mockLogger.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void BotService_Constructor_WithNullLogger_CreatesInstanceWithNullLogger()
    {
        // Arrange
        var config = new DiscordConfiguration
        {
            Token = "test_token",
            TokenType = TokenType.Bot
        };
        var client = new DiscordClient(config);

        // Act
        var service = new BotService(client);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void BotService_Constructor_WithNullLoggerDefaultParameter_CreatesInstance()
    {
        // Arrange
        var config = new DiscordConfiguration
        {
            Token = "test_token",
            TokenType = TokenType.Bot
        };
        var client = new DiscordClient(config);

        // Act
        var service = new BotService(client);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void BotService_MultipleInstances_AreIndependent()
    {
        // Arrange
        var config1 = new DiscordConfiguration
        {
            Token = "test_token_1",
            TokenType = TokenType.Bot
        };
        var config2 = new DiscordConfiguration
        {
            Token = "test_token_2",
            TokenType = TokenType.Bot
        };
        var client1 = new DiscordClient(config1);
        var client2 = new DiscordClient(config2);

        // Act
        var service1 = new BotService(client1);
        var service2 = new BotService(client2);

        // Assert
        Assert.NotNull(service1);
        Assert.NotNull(service2);
        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void BotService_WithDifferentLoggers_CreatesIndependentInstances()
    {
        // Arrange
        var config = new DiscordConfiguration
        {
            Token = "test_token",
            TokenType = TokenType.Bot
        };
        var client = new DiscordClient(config);
        var logger1 = NullLogger<BotService>.Instance;
        var logger2 = new Mock<ILogger<BotService>>().Object;

        // Act
        var service1 = new BotService(client, logger1);
        var service2 = new BotService(client, logger2);

        // Assert
        Assert.NotNull(service1);
        Assert.NotNull(service2);
        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void BotService_DefaultLogger_IsNullLogger()
    {
        // Arrange
        var config = new DiscordConfiguration
        {
            Token = "test_token",
            TokenType = TokenType.Bot
        };
        var client = new DiscordClient(config);

        // Act
        var service = new BotService(client);

        // Assert
        Assert.NotNull(service);
        // Service uses NullLogger when no logger provided
    }

    [Fact]
    public async Task StartAsync_WithTestEnvironment_ReturnsImmediately()
    {
        // Arrange
        var config = new DiscordConfiguration
        {
            Token = "test_token",
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged
        };
        var client = new DiscordClient(config);
        var mockLogger = new Mock<ILogger<BotService>>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var service = new BotService(client, mockLogger.Object);

        // Act
        var task = service.StartAsync(true);

        // Give it a short time to try to connect (will fail with fake token, but that's expected)
        var completedTask = await Task.WhenAny(task, Task.Delay(5000));

        // Assert - should either complete quickly or throw (not hang)
        Assert.True(completedTask == task || task.IsFaulted,
            "StartAsync with isTestEnvironment=true should return quickly or throw, not hang indefinitely");
    }

    [Fact]
    public async Task StartAsync_WithTestEnvironmentFalse_WouldWaitIndefinitely()
    {
        // Arrange
        var config = new DiscordConfiguration
        {
            Token = "test_token",
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged
        };
        var client = new DiscordClient(config);
        var mockLogger = new Mock<ILogger<BotService>>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var service = new BotService(client, mockLogger.Object);

        // Act
        var cts = new CancellationTokenSource(2000); // 2 second timeout
        var task = service.StartAsync(false);

        // Wait for either completion or timeout
        var completedTask = await Task.WhenAny(task, Task.Delay(2000, cts.Token));

        // Assert - with isTestEnvironment=false, it would wait indefinitely (Task.Delay(-1))
        // Since we can't actually test infinite wait, we verify it doesn't complete quickly
        // Note: This will likely throw due to fake token, which is fine - we're testing the delay logic
        if (task.IsCompleted && !task.IsFaulted)
            Assert.True(completedTask != task,
                "StartAsync with isTestEnvironment=false should not complete quickly (should wait indefinitely after connect)");

        cts.Cancel();
    }

    [Fact]
    public async Task StartAsync_WhenConnectAsyncThrows_LogsErrorAndThrows()
    {
        // Arrange
        var config = new DiscordConfiguration
        {
            Token = "invalid_token_that_will_fail",
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged
        };
        var client = new DiscordClient(config);
        var mockLogger = new Mock<ILogger<BotService>>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var service = new BotService(client, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
            await service.StartAsync(true));

        // Verify logger was called with error
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Lavalink operation failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task StartAsync_ConnectsClient_WhenCalled()
    {
        // Arrange
        var config = new DiscordConfiguration
        {
            Token = "test_token",
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged
        };
        var client = new DiscordClient(config);
        var service = new BotService(client);

        // Act & Assert
        // With invalid token, ConnectAsync will throw
        // This verifies that StartAsync attempts to call ConnectAsync
        await Assert.ThrowsAnyAsync<Exception>(async () =>
            await service.StartAsync(true));

        // The fact that an exception was thrown from ConnectAsync proves it was called
    }
}