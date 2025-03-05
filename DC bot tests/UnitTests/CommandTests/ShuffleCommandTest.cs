using DC_bot.Commands;
using DC_bot.Helper;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class ShuffleCommandTest
{
    private readonly Mock<IUserValidationService> _userValidationMock;
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<ILocalizationService> _localizationServiceMock;
    private readonly ShuffleCommand _shuffleCommand;

    public ShuffleCommandTest()
    {
        Mock<ILogger<ShuffleCommand>> mockLogger = new();
        
        _userValidationMock = new Mock<IUserValidationService>();
        _musicQueueServiceMock = new Mock<IMusicQueueService>();
        _localizationServiceMock = new Mock<ILocalizationService>();
        _messageMock = new Mock<IDiscordMessage>();
        _channelMock = new Mock<IDiscordChannel>();
        _guildMock = new Mock<IDiscordGuild>();

        _localizationServiceMock.Setup(l => l.Get("shuffle_command_description"))
            .Returns("Shuffle the playlist.");
        
        _shuffleCommand = new ShuffleCommand(
            _userValidationMock.Object,
            _musicQueueServiceMock.Object,
            mockLogger.Object, _localizationServiceMock.Object
        );
    }

   [Fact]
    public async Task ExecuteAsync_EmptyQueue_SendsErrorMessage()
    {
        // Arrange
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);
        _channelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _guildMock.Setup(g => g.Id).Returns(It.IsAny<ulong>());
        
        _userValidationMock
            .Setup(v => v.ValidateUserAsync(It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new ValidationResult(true));
        
        _musicQueueServiceMock
            .Setup(m => m.GetQueue(It.IsAny<ulong>()))
            .Returns(new Queue<ILavaLinkTrack>());

        _localizationServiceMock.Setup(l => l.Get("shuffle_command_error"))
            .Returns("There is no music in queue.");

        // Act
        await _shuffleCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _messageMock.Verify(m => m.RespondAsync("❌ There is no music in queue."), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidQueue_ShufflesQueue()
    {
        // Arrange
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);
        _channelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _guildMock.Setup(g => g.Id).Returns(It.IsAny<ulong>());
        
        _userValidationMock
            .Setup(v => v.ValidateUserAsync(It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new ValidationResult(true));

        _localizationServiceMock.Setup(l => l.Get("shuffle_command_response"))
            .Returns("The list has been shuffled.");
        
        var originalQueue = new Queue<ILavaLinkTrack>(new List<ILavaLinkTrack>
        {
            Mock.Of<ILavaLinkTrack>(), Mock.Of<ILavaLinkTrack>(), Mock.Of<ILavaLinkTrack>()
        });

        _musicQueueServiceMock
            .Setup(m => m.GetQueue(It.IsAny<ulong>()))
            .Returns(originalQueue);

        // Act
        await _shuffleCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _musicQueueServiceMock.Verify(m => m.SetQueue(It.IsAny<ulong>(), It.IsAny<Queue<ILavaLinkTrack>>()),
            Times.Once);

        // Ellenőrizzük, hogy tényleg módosult-e a queue sorrendje
        _musicQueueServiceMock.Verify(m => m.SetQueue(It.IsAny<ulong>(),
            It.Is<Queue<ILavaLinkTrack>>(q => !q.SequenceEqual(originalQueue))), Times.Once);
        _messageMock.Verify(m => m.RespondAsync("🔀 The list has been shuffled."), Times.Once);
    }
    
    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal("shuffle", _shuffleCommand.Name);
        Assert.Equal("Shuffle the playlist.",_shuffleCommand.Description);
    }
}