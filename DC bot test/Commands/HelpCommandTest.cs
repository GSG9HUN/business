using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DC_bot.Commands;
using DC_bot.Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DC_bot.Tests.Commands
{
    public class HelpCommandTests
    {
        private readonly Mock<ILogger<HelpCommand>> _mockLogger;
        private readonly Mock<IMessageWrapper> _mockMessage;
        private readonly Mock<IServiceLocator> _mockServiceLocator;
        private readonly HelpCommand _helpCommand;

        public HelpCommandTests()
        {
            _mockLogger = new Mock<ILogger<HelpCommand>>();
            _mockMessage = new Mock<IMessageWrapper>();
            _mockServiceLocator = new Mock<IServiceLocator>();
            
            _helpCommand = new HelpCommand(_mockLogger.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldListCommands_WhenCalled()
        {
            // Arrange
            var mockCommand1 = new Mock<ICommand>();
            mockCommand1.Setup(c => c.Name).Returns("ping");
            mockCommand1.Setup(c => c.Description).Returns("Replies with Pong!");
            
            var mockCommand2 = new Mock<ICommand>();
            mockCommand2.Setup(c => c.Name).Returns("play");
            mockCommand2.Setup(c => c.Description).Returns("Plays a song");
            
            var commands = new List<ICommand> { mockCommand1.Object, mockCommand2.Object };
            _mockServiceLocator.Setup(s => s.GetServices<ICommand>()).Returns(commands);
            
            // Act
            await _helpCommand.ExecuteAsync(_mockMessage.Object);

            // Assert
            _mockMessage.Verify(m => m.RespondAsync("Available commands:\n" +
                                                    "ping : Replies with Pong!\n" +
                                                    "play : Plays a song\n"), Times.Once);
        }

        [Fact]
        public void METHOD()
        {
            
        }
    }
}