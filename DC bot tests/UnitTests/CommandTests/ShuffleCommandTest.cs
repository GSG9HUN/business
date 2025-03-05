using DC_bot.Commands;
using DC_bot.Helper;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class ShuffleCommandTest
{
    private readonly Mock<IUserValidationService> _mockUserValidation;
    private readonly Mock<MusicQueueService> _mockMusicQueueService;
    private readonly Mock<IDiscordMessage> _mockMessage;
    private readonly ShuffleCommand _shuffleCommand;

    public ShuffleCommandTest()
    {
        Mock<ILogger<ShuffleCommand>> mockLogger = new();
        Mock<ILocalizationService> localizationServiceMock = new();

        _mockUserValidation = new Mock<IUserValidationService>();
        _mockMusicQueueService = new Mock<MusicQueueService>();
        _mockMessage = new Mock<IDiscordMessage>();

        _shuffleCommand = new ShuffleCommand(
            _mockUserValidation.Object,
            _mockMusicQueueService.Object,
            mockLogger.Object, localizationServiceMock.Object
        );
    }

   /* [Fact]
    public async Task ExecuteAsync_EmptyQueue_SendsErrorMessage()
    {
        // Arrange
        _mockUserValidation
            .Setup(v => v.ValidateUserAsync(It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new ValidationResult(true));

        _mockMusicQueueService
            .Setup(m => m.GetQueue(It.IsAny<ulong>()))
            .Returns(new Queue<ILavaLinkTrack>());

        // Act
        await _shuffleCommand.ExecuteAsync(_mockMessage.Object);

        // Assert
        _mockMessage.Verify(m => m.RespondAsync("❌ Nincs elérhető zene a várólistában!"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidQueue_ShufflesQueue()
    {
        // Arrange
        _mockUserValidation
            .Setup(v => v.ValidateUserAsync(It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new ValidationResult(true));

        var originalQueue = new Queue<ILavaLinkTrack>(new List<ILavaLinkTrack>
        {
            Mock.Of<ILavaLinkTrack>(), Mock.Of<ILavaLinkTrack>(), Mock.Of<ILavaLinkTrack>()
        });

        _mockMusicQueueService
            .Setup(m => m.GetQueue(It.IsAny<ulong>()))
            .Returns(originalQueue);

        // Act
        await _shuffleCommand.ExecuteAsync(_mockMessage.Object);

        // Assert
        _mockMusicQueueService.Verify(m => m.SetQueue(It.IsAny<ulong>(), It.IsAny<Queue<ILavaLinkTrack>>()),
            Times.Once);

        // Ellenőrizzük, hogy tényleg módosult-e a queue sorrendje
        _mockMusicQueueService.Verify(m => m.SetQueue(It.IsAny<ulong>(),
            It.Is<Queue<ILavaLinkTrack>>(q => !q.SequenceEqual(originalQueue))), Times.Once);
    }*/
}