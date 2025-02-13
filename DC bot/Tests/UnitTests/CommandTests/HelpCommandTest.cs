using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DC_bot.Tests.UnitTests.CommandTests;

public class HelpCommandTests
{
    private readonly Mock<ILogger<HelpCommand>> _mockLogger;
    private readonly Mock<IDiscordMessageWrapper> _mockMessage;
    private readonly HelpCommand _helpCommand;

    public HelpCommandTests()
    {
        _mockLogger = new Mock<ILogger<HelpCommand>>();
        _mockMessage = new Mock<IDiscordMessageWrapper>();

        var services = new ServiceCollection();
        var mockCommand1 = new Mock<ICommand>();
        mockCommand1.Setup(c => c.Name).Returns("ping");
        mockCommand1.Setup(c => c.Description).Returns("Replies with Pong!");

        var mockCommand2 = new Mock<ICommand>();
        mockCommand2.Setup(c => c.Name).Returns("play");
        mockCommand2.Setup(c => c.Description).Returns("Plays a song");

        services.AddSingleton(mockCommand1.Object);
        services.AddSingleton(mockCommand2.Object);

        var serviceProvider = services.BuildServiceProvider();
        ServiceLocator.SetServiceProvider(serviceProvider);

        _helpCommand = new HelpCommand(_mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldListCommands_WhenCalled()
    {
        // Act
        await _helpCommand.ExecuteAsync(_mockMessage.Object);

        // Assert
        _mockMessage.Verify(m => m.RespondAsync("Available commands:\n" +
                                                "ping : Replies with Pong!\n" +
                                                "play : Plays a song\n"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleNoCommands()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        ServiceLocator.SetServiceProvider(serviceProvider);

        // Act
        await _helpCommand.ExecuteAsync(_mockMessage.Object);

        // Assert
        _mockMessage.Verify(m => m.RespondAsync("Available commands:\n"), Times.Once);
    }
}