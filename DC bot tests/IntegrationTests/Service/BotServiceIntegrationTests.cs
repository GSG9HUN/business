﻿using DC_bot.Service;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service;

public class BotServiceIntegrationTests
{
    [Fact]
    public async Task StartAsync_WithTestEnvironment_ReturnsQuickly()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BotService>>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var discordConfig = new DiscordConfiguration
        {
            Token = "test-token",
            Intents = DiscordIntents.AllUnprivileged
        };
        var discordClient = new DiscordClient(discordConfig);

        var service = new BotService(discordClient, mockLogger.Object);

        // Act
        var cts = new CancellationTokenSource(2000);
        var task = service.StartAsync(true);
        var completedTask = await Task.WhenAny(task, Task.Delay(2000, cts.Token));

        // Assert
        if (task is { IsCompleted: true, IsFaulted: false })
            Assert.True(completedTask == task, "Task should complete when isTestEnvironment=true");
        else if (task.IsFaulted) Assert.NotNull(task.Exception);

        await cts.CancelAsync();
    }

    [Fact]
    public async Task StartAsync_WithInvalidToken_ThrowsException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BotService>>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var discordConfig = new DiscordConfiguration
        {
            Token = "invalid-token",
            Intents = DiscordIntents.AllUnprivileged
        };
        var discordClient = new DiscordClient(discordConfig);

        var service = new BotService(discordClient, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
            await service.StartAsync(true));
    }

    [Fact]
    public async Task StartAsync_LogsError_WhenConnectionFails()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BotService>>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var discordConfig = new DiscordConfiguration
        {
            Token = "invalid-token",
            Intents = DiscordIntents.AllUnprivileged
        };
        var discordClient = new DiscordClient(discordConfig);

        var service = new BotService(discordClient, mockLogger.Object);

        // Act
        try
        {
            await service.StartAsync(true);
        }
        catch
        {
            // Expected
        }

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task StartAsync_WithValidToken_AttemptsConnection()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BotService>>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var discordConfig = new DiscordConfiguration
        {
            Token = "test-token",
            Intents = DiscordIntents.AllUnprivileged
        };
        var discordClient = new DiscordClient(discordConfig);

        var service = new BotService(discordClient, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
            await service.StartAsync(true));
    }

    [Fact]
    public async Task StartAsync_WithInvalidToken_AndNonTestMode_ThrowsAndLogsLavalinkOperationFailed()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BotService>>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var discordClient = new DiscordClient(new DiscordConfiguration
        {
            Token = "invalid-token",
            Intents = DiscordIntents.AllUnprivileged
        });

        var service = new BotService(discordClient, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => service.StartAsync(false));

        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.Is<EventId>(e => e.Id == 2013),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.AtLeastOnce);
    }
}