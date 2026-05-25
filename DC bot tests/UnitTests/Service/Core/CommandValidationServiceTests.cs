using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Core;

[Trait("Category", "Unit")]
public class CommandValidationServiceTests
{
    private readonly CommandValidationService _commandValidationService = new();
    private readonly ILogger _mockLogger = NullLogger.Instance;
    private readonly Mock<IDiscordMessage> _mockMessage = new();
    private readonly Mock<IResponseBuilder> _mockResponseBuilder = new();
    private readonly Mock<IUserValidationService> _mockUserValidation = new();

    #region TryValidateUserAsync Tests

    [Fact]
    public async Task TryValidateUserAsync_ValidUser_ReturnsValidationResult()
    {
        var mockMember = new Mock<IDiscordMember>();
        var validResult = new UserValidationResult(true, string.Empty, mockMember.Object);

        _mockUserValidation
            .Setup(x => x.ValidateUserAsync(_mockMessage.Object))
            .ReturnsAsync(validResult);

        var result = await _commandValidationService.TryValidateUserAsync(
            _mockUserValidation.Object,
            _mockResponseBuilder.Object,
            _mockMessage.Object);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.Equal(mockMember.Object, result.Member);
        _mockResponseBuilder.Verify(x => x.SendValidationErrorAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task TryValidateUserAsync_InvalidUser_ReturnsNull()
    {
        const string errorKey = "user_not_in_voice_channel";
        var invalidResult = new UserValidationResult(false, errorKey);

        _mockUserValidation
            .Setup(x => x.ValidateUserAsync(_mockMessage.Object))
            .ReturnsAsync(invalidResult);

        _mockResponseBuilder
            .Setup(x => x.SendValidationErrorAsync(_mockMessage.Object, errorKey))
            .Returns(Task.CompletedTask);

        var result = await _commandValidationService.TryValidateUserAsync(
            _mockUserValidation.Object,
            _mockResponseBuilder.Object,
            _mockMessage.Object);

        Assert.Null(result);
        _mockResponseBuilder.Verify(x => x.SendValidationErrorAsync(_mockMessage.Object, errorKey), Times.Once);
    }

    [Fact]
    public async Task TryValidateUserAsync_SendsErrorMessage_WhenValidationFails()
    {
        const string errorKey = "bot_not_connected";
        var invalidResult = new UserValidationResult(false, errorKey);

        _mockUserValidation
            .Setup(x => x.ValidateUserAsync(_mockMessage.Object))
            .ReturnsAsync(invalidResult);

        _mockResponseBuilder
            .Setup(x => x.SendValidationErrorAsync(_mockMessage.Object, errorKey))
            .Returns(Task.CompletedTask);

        await _commandValidationService.TryValidateUserAsync(
            _mockUserValidation.Object,
            _mockResponseBuilder.Object,
            _mockMessage.Object);

        _mockResponseBuilder.Verify(x => x.SendValidationErrorAsync(_mockMessage.Object, errorKey), Times.Once);
    }

    #endregion

    #region TryGetArgumentAsync Tests

    [Fact]
    public async Task TryGetArgumentAsync_WithArguments_ReturnsArgument()
    {
        const string commandContent = "!play https://youtube.com/watch?v=test";
        _mockMessage.Setup(x => x.Content).Returns(commandContent);

        var result = await _commandValidationService.TryGetArgumentAsync(
            _mockMessage.Object,
            _mockResponseBuilder.Object,
            _mockLogger,
            "play");

        Assert.NotNull(result);
        Assert.Equal("https://youtube.com/watch?v=test", result);
        _mockResponseBuilder.Verify(x => x.SendUsageAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task TryGetArgumentAsync_WithoutArguments_ReturnsNull()
    {
        const string commandContent = "!play";
        _mockMessage.Setup(x => x.Content).Returns(commandContent);

        _mockResponseBuilder
            .Setup(x => x.SendUsageAsync(_mockMessage.Object, "play"))
            .Returns(Task.CompletedTask);

        var result = await _commandValidationService.TryGetArgumentAsync(
            _mockMessage.Object,
            _mockResponseBuilder.Object,
            _mockLogger,
            "play");

        Assert.Null(result);
        _mockResponseBuilder.Verify(x => x.SendUsageAsync(_mockMessage.Object, "play"), Times.Once);
    }

    [Fact]
    public async Task TryGetArgumentAsync_WithMultipleWords_ReturnsEntireArgument()
    {
        const string commandContent = "!play never gonna give you up";
        _mockMessage.Setup(x => x.Content).Returns(commandContent);

        var result = await _commandValidationService.TryGetArgumentAsync(
            _mockMessage.Object,
            _mockResponseBuilder.Object,
            _mockLogger,
            "play");

        Assert.NotNull(result);
        Assert.Equal("never gonna give you up", result);
    }

    [Fact]
    public async Task TryGetArgumentAsync_WithWhitespace_TrimsArgument()
    {
        const string commandContent = "!skip   extra spaces  ";
        _mockMessage.Setup(x => x.Content).Returns(commandContent);

        var result = await _commandValidationService.TryGetArgumentAsync(
            _mockMessage.Object,
            _mockResponseBuilder.Object,
            _mockLogger,
            "skip");

        Assert.NotNull(result);
        Assert.Equal("extra spaces", result);
    }

    [Fact]
    public async Task TryGetArgumentAsync_EmptyCommand_ReturnsNull()
    {
        const string commandContent = "!";
        _mockMessage.Setup(x => x.Content).Returns(commandContent);

        _mockResponseBuilder
            .Setup(x => x.SendUsageAsync(_mockMessage.Object, "test"))
            .Returns(Task.CompletedTask);

        var result = await _commandValidationService.TryGetArgumentAsync(
            _mockMessage.Object,
            _mockResponseBuilder.Object,
            _mockLogger,
            "test");

        Assert.Null(result);
    }

    [Fact]
    public async Task TryGetArgumentAsync_WithSpecialCharacters_ReturnsCorrectArgument()
    {
        const string commandContent = "!tag create test_tag Content with #special $chars!";
        _mockMessage.Setup(x => x.Content).Returns(commandContent);

        var result = await _commandValidationService.TryGetArgumentAsync(
            _mockMessage.Object,
            _mockResponseBuilder.Object,
            _mockLogger,
            "tag");

        Assert.NotNull(result);
        Assert.Equal("create test_tag Content with #special $chars!", result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CommandValidationService_ValidUserWithArguments_BothSucceed()
    {
        var mockMember = new Mock<IDiscordMember>();
        var validResult = new UserValidationResult(true, string.Empty, mockMember.Object);
        const string commandContent = "!play test song";

        _mockUserValidation
            .Setup(x => x.ValidateUserAsync(_mockMessage.Object))
            .ReturnsAsync(validResult);

        _mockMessage.Setup(x => x.Content).Returns(commandContent);

        var userResult = await _commandValidationService.TryValidateUserAsync(
            _mockUserValidation.Object,
            _mockResponseBuilder.Object,
            _mockMessage.Object);

        var argResult = await _commandValidationService.TryGetArgumentAsync(
            _mockMessage.Object,
            _mockResponseBuilder.Object,
            _mockLogger,
            "play");

        Assert.NotNull(userResult);
        Assert.True(userResult.IsValid);
        Assert.NotNull(argResult);
        Assert.Equal("test song", argResult);
    }

    [Fact]
    public async Task CommandValidationService_InvalidUser_SkipsArgumentValidation()
    {
        var invalidResult = new UserValidationResult(false, "error");

        _mockUserValidation
            .Setup(x => x.ValidateUserAsync(_mockMessage.Object))
            .ReturnsAsync(invalidResult);

        _mockResponseBuilder
            .Setup(x => x.SendValidationErrorAsync(_mockMessage.Object, "error"))
            .Returns(Task.CompletedTask);

        var userResult = await _commandValidationService.TryValidateUserAsync(
            _mockUserValidation.Object,
            _mockResponseBuilder.Object,
            _mockMessage.Object);

        Assert.Null(userResult);
        _mockResponseBuilder.Verify(x => x.SendValidationErrorAsync(_mockMessage.Object, "error"), Times.Once);
    }

    #endregion
}
