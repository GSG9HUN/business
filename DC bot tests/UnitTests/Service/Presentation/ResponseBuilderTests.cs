using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Service.Presentation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Presentation;

public class ResponseBuilderTests
{
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private readonly Mock<IDiscordMessage> _mockMessage;
    private readonly ResponseBuilder _responseBuilder;

    public ResponseBuilderTests()
    {
        _mockLocalizationService = new Mock<ILocalizationService>();
        _responseBuilder = new ResponseBuilder(_mockLocalizationService.Object, NullLogger<ResponseBuilder>.Instance);
        _mockMessage = new Mock<IDiscordMessage>();
    }

    #region Exception Handling Tests

    [Fact]
    public async Task ResponseBuilder_MessageRespondThrows_DoesNotThrowException()
    {
        // Arrange
        const string commandName = "test";

        _mockLocalizationService
            .Setup(x => x.Get(It.IsAny<string>()))
            .Returns("Test message");

        _mockMessage
            .Setup(x => x.RespondAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Discord API error"));

        // Act & Assert - Should not throw
        await _responseBuilder.SendCommandResponseAsync(_mockMessage.Object, commandName);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task ResponseBuilder_SendMultipleResponses_SendsAllMessages()
    {
        // Arrange
        _mockLocalizationService
            .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] _) => $"Message for {key}");

        _mockMessage
            .Setup(x => x.RespondAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _responseBuilder.SendValidationErrorAsync(_mockMessage.Object, "error1");
        await _responseBuilder.SendSuccessAsync(_mockMessage.Object, "Success!");
        await _responseBuilder.SendCommandResponseAsync(_mockMessage.Object, "test");

        // Assert
        _mockMessage.Verify(x => x.RespondAsync(It.IsAny<string>()), Times.Exactly(3));
    }

    #endregion

    #region SendValidationErrorAsync Tests

    [Fact]
    public async Task SendValidationErrorAsync_ValidErrorKey_SendsMessage()
    {
        // Arrange
        const string errorKey = "user_not_in_voice_channel";
        const string errorMessage = "You must be in a voice channel!";

        _mockLocalizationService
            .Setup(x => x.Get(errorKey))
            .Returns(errorMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(errorMessage))
            .Returns(Task.CompletedTask);

        // Act
        await _responseBuilder.SendValidationErrorAsync(_mockMessage.Object, errorKey);

        // Assert
        _mockMessage.Verify(x => x.RespondAsync(errorMessage), Times.Once);
    }

    [Fact]
    public async Task SendValidationErrorAsync_EmptyErrorKey_DoesNotSend()
    {
        // Arrange
        const string errorKey = "";

        // Act
        await _responseBuilder.SendValidationErrorAsync(_mockMessage.Object, errorKey);

        // Assert
        _mockMessage.Verify(x => x.RespondAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendValidationErrorAsync_NullErrorKey_DoesNotSend()
    {
        // Arrange
        string? errorKey = null;

        // Act
        await _responseBuilder.SendValidationErrorAsync(_mockMessage.Object, errorKey!);

        // Assert
        _mockMessage.Verify(x => x.RespondAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendValidationErrorAsync_MessageSendFails_HandlesSilently()
    {
        // Arrange
        const string errorKey = "test_error";
        const string errorMessage = "Test error message";

        _mockLocalizationService
            .Setup(x => x.Get(errorKey))
            .Returns(errorMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(errorMessage))
            .ThrowsAsync(new Exception("Discord API error"));

        // Act
        await _responseBuilder.SendValidationErrorAsync(_mockMessage.Object, errorKey);

        // Assert
        _mockMessage.Verify(x => x.RespondAsync(errorMessage), Times.Once);
    }

    #endregion

    #region SendUsageAsync Tests

    [Fact]
    public async Task SendUsageAsync_ValidCommandName_SendsUsageMessage()
    {
        // Arrange
        const string commandName = "play";
        const string usageMessage = "Usage: !play <url or search query>";

        _mockLocalizationService
            .Setup(x => x.Get("play_command_usage"))
            .Returns(usageMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(usageMessage))
            .Returns(Task.CompletedTask);

        // Act
        await _responseBuilder.SendUsageAsync(_mockMessage.Object, commandName);

        // Assert
        _mockMessage.Verify(x => x.RespondAsync(usageMessage), Times.Once);
        _mockLocalizationService.Verify(x => x.Get("play_command_usage"), Times.Once);
    }

    [Fact]
    public async Task SendUsageAsync_MultipleCommands_SendsCorrectUsage()
    {
        // Arrange
        var commands = new[] { "pause", "skip", "resume" };

        foreach (var cmd in commands)
            _mockLocalizationService
                .Setup(x => x.Get($"{cmd}_command_usage"))
                .Returns($"Usage: !{cmd}");

        // Act & Assert
        foreach (var cmd in commands)
        {
            _mockMessage.Reset();
            _mockMessage
                .Setup(x => x.RespondAsync($"Usage: !{cmd}"))
                .Returns(Task.CompletedTask);

            await _responseBuilder.SendUsageAsync(_mockMessage.Object, cmd);

            _mockMessage.Verify(x => x.RespondAsync($"Usage: !{cmd}"), Times.Once);
        }
    }

    #endregion

    #region SendSuccessAsync Tests

    [Fact]
    public async Task SendSuccessAsync_ValidText_SendsMessage()
    {
        // Arrange
        const string successMessage = "Track added to queue!";

        _mockMessage
            .Setup(x => x.RespondAsync(successMessage))
            .Returns(Task.CompletedTask);

        // Act
        await _responseBuilder.SendSuccessAsync(_mockMessage.Object, successMessage);

        // Assert
        _mockMessage.Verify(x => x.RespondAsync(successMessage), Times.Once);
    }

    [Fact]
    public async Task SendSuccessAsync_EmptyText_SendsEmptyMessage()
    {
        // Arrange
        const string successMessage = "";

        _mockMessage
            .Setup(x => x.RespondAsync(successMessage))
            .Returns(Task.CompletedTask);

        // Act
        await _responseBuilder.SendSuccessAsync(_mockMessage.Object, successMessage);

        // Assert
        _mockMessage.Verify(x => x.RespondAsync(successMessage), Times.Once);
    }

    [Fact]
    public async Task SendSuccessAsync_LongText_SendsEntireMessage()
    {
        // Arrange
        var longMessage = new string('x', 2000);

        _mockMessage
            .Setup(x => x.RespondAsync(longMessage))
            .Returns(Task.CompletedTask);

        // Act
        await _responseBuilder.SendSuccessAsync(_mockMessage.Object, longMessage);

        // Assert
        _mockMessage.Verify(x => x.RespondAsync(longMessage), Times.Once);
    }

    #endregion

    #region SendCommandResponseAsync Tests

    [Fact]
    public async Task SendCommandResponseAsync_ValidCommandName_SendsResponse()
    {
        // Arrange
        const string commandName = "clear";
        const string responseMessage = "Queue cleared successfully!";

        _mockLocalizationService
            .Setup(x => x.Get("clear_command_response"))
            .Returns(responseMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(responseMessage))
            .Returns(Task.CompletedTask);

        // Act
        await _responseBuilder.SendCommandResponseAsync(_mockMessage.Object, commandName);

        // Assert
        _mockMessage.Verify(x => x.RespondAsync(responseMessage), Times.Once);
        _mockLocalizationService.Verify(x => x.Get("clear_command_response"), Times.Once);
    }

    [Fact]
    public async Task SendCommandResponseAsync_BuildsCorrectLocalizationKey()
    {
        // Arrange
        const string commandName = "shuffle";
        var expectedKey = $"{commandName}_command_response";

        _mockLocalizationService
            .Setup(x => x.Get(expectedKey))
            .Returns("Queue shuffled!");

        _mockMessage
            .Setup(x => x.RespondAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _responseBuilder.SendCommandResponseAsync(_mockMessage.Object, commandName);

        // Assert
        _mockLocalizationService.Verify(x => x.Get(expectedKey), Times.Once);
    }

    #endregion

    #region SendCommandErrorResponse Tests

    [Fact]
    public async Task SendCommandErrorResponse_ValidCommandName_SendsErrorMessage()
    {
        // Arrange
        const string commandName = "pause";
        const string errorMessage = "No track is currently playing!";

        _mockLocalizationService
            .Setup(x => x.Get("pause_command_error"))
            .Returns(errorMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(errorMessage))
            .Returns(Task.CompletedTask);

        // Act
        await _responseBuilder.SendCommandErrorResponse(_mockMessage.Object, commandName);

        // Assert
        _mockMessage.Verify(x => x.RespondAsync(errorMessage), Times.Once);
    }

    [Fact]
    public async Task SendCommandErrorResponse_BuildsCorrectErrorKey()
    {
        // Arrange
        const string commandName = "skip";
        var expectedKey = $"{commandName}_command_error";

        _mockLocalizationService
            .Setup(x => x.Get(expectedKey))
            .Returns("Cannot skip!");

        _mockMessage
            .Setup(x => x.RespondAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _responseBuilder.SendCommandErrorResponse(_mockMessage.Object, commandName);

        // Assert
        _mockLocalizationService.Verify(x => x.Get(expectedKey), Times.Once);
    }

    #endregion
}