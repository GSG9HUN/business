using DC_bot.Interface;
using DC_bot.Service;
using DC_bot.Wrapper;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DC_bot.Tests.UnitTests.Wrapper;

public class SingletonDiscordClientTest
{
    [Fact]
    public void InitializeLogger_Should_Log_Message()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SingletonDiscordClient>?>();

        // Act
        SingletonDiscordClient.InitializeLogger(loggerMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Logger initialized")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    [Fact]
    public async Task OnGuildAvailable_Should_Initialize_Services()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SingletonDiscordClient>?>();
        SingletonDiscordClient.InitializeLogger(loggerMock.Object);

        var mockMusicQueueService = new Mock<MusicQueueService>();
        var mockLavaLinkService = new Mock<ILavaLinkService>();

        /*ServiceLocator.SetService<MusicQueueService>(mockMusicQueueService.Object);
        ServiceLocator.SetService<ILavaLinkService>(mockLavaLinkService.Object);

        var guild = new DiscordGuild { Id = 123456789, Name = "TestGuild" };
        var eventArgs = new GuildCreateEventArgs { Guild = guild };*/

        // Act
       /*typeof(SingletonDiscordClient)
            .GetMethod("OnGuildAvailable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { null, eventArgs });
        */
        // Assert
        mockMusicQueueService.Verify(mq => mq.Init(123456789), Times.Once);
        mockLavaLinkService.Verify(ll => ll.Init(123456789), Times.Once);
    }
}