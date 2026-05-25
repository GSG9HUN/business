using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Service.Presentation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Presentation;

[Trait("Category", "Unit")]
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
        const string commandName = "test";

        _mockLocalizationService
            .Setup(x => x.Get(It.IsAny<string>()))
            .Returns("Test message");

        _mockMessage
            .Setup(x => x.RespondAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Discord API error"));

        await _responseBuilder.SendCommandResponseAsync(_mockMessage.Object, commandName);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task ResponseBuilder_SendMultipleResponses_SendsAllMessages()
    {
        _mockLocalizationService
            .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] _) => $"Message for {key}");

        _mockMessage
            .Setup(x => x.RespondAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendValidationErrorAsync(_mockMessage.Object, "error1");
        await _responseBuilder.SendSuccessAsync(_mockMessage.Object, "Success!");
        await _responseBuilder.SendCommandResponseAsync(_mockMessage.Object, "test");

        _mockMessage.Verify(x => x.RespondAsync(It.IsAny<string>()), Times.Exactly(3));
    }

    #endregion

    #region SendValidationErrorAsync Tests

    [Fact]
    public async Task SendValidationErrorAsync_ValidErrorKey_SendsMessage()
    {
        const string errorKey = "user_not_in_voice_channel";
        const string errorMessage = "You must be in a voice channel!";

        _mockLocalizationService
            .Setup(x => x.Get(errorKey))
            .Returns(errorMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(errorMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendValidationErrorAsync(_mockMessage.Object, errorKey);

        _mockMessage.Verify(x => x.RespondAsync(errorMessage), Times.Once);
    }

    [Fact]
    public async Task SendValidationErrorAsync_EmptyErrorKey_DoesNotSend()
    {
        const string errorKey = "";

        await _responseBuilder.SendValidationErrorAsync(_mockMessage.Object, errorKey);

        _mockMessage.Verify(x => x.RespondAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendValidationErrorAsync_NullErrorKey_DoesNotSend()
    {
        string? errorKey = null;

        await _responseBuilder.SendValidationErrorAsync(_mockMessage.Object, errorKey!);

        _mockMessage.Verify(x => x.RespondAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendValidationErrorAsync_MessageSendFails_HandlesSilently()
    {
        const string errorKey = "test_error";
        const string errorMessage = "Test error message";

        _mockLocalizationService
            .Setup(x => x.Get(errorKey))
            .Returns(errorMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(errorMessage))
            .ThrowsAsync(new Exception("Discord API error"));

        await _responseBuilder.SendValidationErrorAsync(_mockMessage.Object, errorKey);

        _mockMessage.Verify(x => x.RespondAsync(errorMessage), Times.Once);
    }

    #endregion

    #region SendUsageAsync Tests

    [Fact]
    public async Task SendUsageAsync_ValidCommandName_SendsUsageMessage()
    {
        const string commandName = "play";
        const string usageMessage = "Usage: !play <url or search query>";

        _mockLocalizationService
            .Setup(x => x.Get("play_command_usage"))
            .Returns(usageMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(usageMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendUsageAsync(_mockMessage.Object, commandName);

        _mockMessage.Verify(x => x.RespondAsync(usageMessage), Times.Once);
        _mockLocalizationService.Verify(x => x.Get("play_command_usage"), Times.Once);
    }

    [Fact]
    public async Task SendUsageAsync_MultipleCommands_SendsCorrectUsage()
    {
        var commands = new[] { "pause", "skip", "resume" };

        foreach (var cmd in commands)
            _mockLocalizationService
                .Setup(x => x.Get($"{cmd}_command_usage"))
                .Returns($"Usage: !{cmd}");

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
        const string successMessage = "Track added to queue!";

        _mockMessage
            .Setup(x => x.RespondAsync(successMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendSuccessAsync(_mockMessage.Object, successMessage);

        _mockMessage.Verify(x => x.RespondAsync(successMessage), Times.Once);
    }

    [Fact]
    public async Task SendSuccessAsync_EmptyText_SendsEmptyMessage()
    {
        const string successMessage = "";

        _mockMessage
            .Setup(x => x.RespondAsync(successMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendSuccessAsync(_mockMessage.Object, successMessage);

        _mockMessage.Verify(x => x.RespondAsync(successMessage), Times.Once);
    }

    [Fact]
    public async Task SendSuccessAsync_LongText_SendsEntireMessage()
    {
        var longMessage = new string('x', 2000);

        _mockMessage
            .Setup(x => x.RespondAsync(longMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendSuccessAsync(_mockMessage.Object, longMessage);

        _mockMessage.Verify(x => x.RespondAsync(longMessage), Times.Once);
    }

    #endregion

    #region SendCommandResponseAsync Tests

    [Fact]
    public async Task SendCommandResponseAsync_ValidCommandName_SendsResponse()
    {
        const string commandName = "clear";
        const string responseMessage = "Queue cleared successfully!";

        _mockLocalizationService
            .Setup(x => x.Get("clear_command_response"))
            .Returns(responseMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(responseMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendCommandResponseAsync(_mockMessage.Object, commandName);

        _mockMessage.Verify(x => x.RespondAsync(responseMessage), Times.Once);
        _mockLocalizationService.Verify(x => x.Get("clear_command_response"), Times.Once);
    }

    [Fact]
    public async Task SendCommandResponseAsync_BuildsCorrectLocalizationKey()
    {
        const string commandName = "shuffle";
        var expectedKey = $"{commandName}_command_response";

        _mockLocalizationService
            .Setup(x => x.Get(expectedKey))
            .Returns("Queue shuffled!");

        _mockMessage
            .Setup(x => x.RespondAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendCommandResponseAsync(_mockMessage.Object, commandName);

        _mockLocalizationService.Verify(x => x.Get(expectedKey), Times.Once);
    }

    #endregion

    #region SendCommandErrorResponse Tests

    [Fact]
    public async Task SendCommandErrorResponse_ValidCommandName_SendsErrorMessage()
    {
        const string commandName = "pause";
        const string errorMessage = "No track is currently playing!";

        _mockLocalizationService
            .Setup(x => x.Get("pause_command_error"))
            .Returns(errorMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(errorMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendCommandErrorResponse(_mockMessage.Object, commandName);

        _mockMessage.Verify(x => x.RespondAsync(errorMessage), Times.Once);
    }

    [Fact]
    public async Task SendCommandErrorResponse_BuildsCorrectErrorKey()
    {
        const string commandName = "skip";
        var expectedKey = $"{commandName}_command_error";

        _mockLocalizationService
            .Setup(x => x.Get(expectedKey))
            .Returns("Cannot skip!");

        _mockMessage
            .Setup(x => x.RespondAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendCommandErrorResponse(_mockMessage.Object, commandName);

        _mockLocalizationService.Verify(x => x.Get(expectedKey), Times.Once);
    }

    #endregion
}
