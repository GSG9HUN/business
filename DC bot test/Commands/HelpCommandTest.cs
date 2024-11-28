using DC_bot.Commands;
using DC_bot.Interface;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DC_bot_test.Commands;

[TestSubject(typeof(HelpCommand))]
public class HelpCommandTest
{

    [Fact]
    public async Task ExecuteAsync_ShouldRespondWithAvailableCommands()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<HelpCommand>>();
        var helpCommand = new HelpCommand(loggerMock.Object);

        // Mock MessageWrapper
        var messageMock = new Mock<IMessageWrapper>();
        messageMock
            .Setup(m => m.RespondAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await helpCommand.ExecuteAsync(messageMock.Object);

        // Assert
        messageMock.Verify(m => m.RespondAsync(It.Is<string>(s => s.Contains("Available commands"))), Times.Once);
        loggerMock.Verify(log => log.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString() == "Help Command executed!"),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }
}