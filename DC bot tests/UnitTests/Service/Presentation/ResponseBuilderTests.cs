using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Service.Presentation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Presentation;

[Trait("Category", "Unit")]
public class ResponseBuilderTests
{
    private const ulong GuildId = 123UL;
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private readonly Mock<IDiscordMessage> _mockMessage;
    private readonly ResponseBuilder _responseBuilder;

    public ResponseBuilderTests()
    {
        _mockLocalizationService = new Mock<ILocalizationService>();
        _mockMessage = new Mock<IDiscordMessage>();

        _mockLocalizationService
            .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<string, object[]>((key, args) => args.Length == 0 ? key : string.Format(key, args));

        _mockLocalizationService
            .Setup(x => x.Get(GuildId, It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<ulong, string, object[]>((_, key, args) => _mockLocalizationService.Object.Get(key, args));

        _responseBuilder = new ResponseBuilder(_mockLocalizationService.Object, NullLogger<ResponseBuilder>.Instance);

        SetupMessageGuild();
    }

    #region Exception Handling Tests

    [Fact]
    public async Task ResponseBuilder_MessageRespondThrows_DoesNotThrowException()
    {
        const string localizationKey = "test_command_response";

        _mockLocalizationService
            .Setup(x => x.Get(localizationKey))
            .Returns("Test message");

        _mockMessage
            .Setup(x => x.RespondAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Discord API error"));

        await _responseBuilder.SendSuccessAsync(_mockMessage.Object, localizationKey);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task ResponseBuilder_SendMultipleResponses_SendsAllMessages()
    {
        _mockMessage
            .Setup(x => x.RespondAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendValidationErrorAsync(_mockMessage.Object, "error1");
        await _responseBuilder.SendSuccessAsync(_mockMessage.Object, "success_key");
        await _responseBuilder.SendWarningAsync(_mockMessage.Object, "warning_key");

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
        _mockLocalizationService.Verify(
            x => x.Get(GuildId, errorKey, It.Is<object[]>(args => args.Length == 0)),
            Times.Once);
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
        const string usageKey = "play_command_usage";
        const string usageMessage = "Usage: !play <url or search query>";

        _mockLocalizationService
            .Setup(x => x.Get(usageKey))
            .Returns(usageMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(usageMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendUsageAsync(_mockMessage.Object, commandName);

        _mockMessage.Verify(x => x.RespondAsync(usageMessage), Times.Once);
        _mockLocalizationService.Verify(
            x => x.Get(GuildId, usageKey, It.Is<object[]>(args => args.Length == 0)),
            Times.Once);
    }

    [Fact]
    public async Task SendUsageAsync_MultipleCommands_SendsCorrectUsage()
    {
        var commands = new[] { "pause", "skip", "resume" };

        foreach (var cmd in commands)
        {
            _mockLocalizationService
                .Setup(x => x.Get($"{cmd}_command_usage"))
                .Returns($"Usage: !{cmd}");
        }

        foreach (var cmd in commands)
        {
            _mockMessage.Reset();
            SetupMessageGuild();
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
    public async Task SendSuccessAsync_ValidLocalizationKey_SendsLocalizedMessage()
    {
        const string successKey = "track_added";
        const string successMessage = "Track added to queue!";

        _mockLocalizationService
            .Setup(x => x.Get(successKey))
            .Returns(successMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(successMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendSuccessAsync(_mockMessage.Object, successKey);

        _mockMessage.Verify(x => x.RespondAsync(successMessage), Times.Once);
    }

    [Fact]
    public async Task SendSuccessAsync_WithArguments_PassesArgumentsToLocalization()
    {
        const string successKey = "playlist_saved";
        const string playlistName = "mix";
        const string successMessage = "Playlist 'mix' saved.";

        _mockLocalizationService
            .Setup(x => x.Get(
                GuildId,
                successKey,
                It.Is<object[]>(args => args.Length == 1 && (string)args[0] == playlistName)))
            .Returns(successMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(successMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendSuccessAsync(_mockMessage.Object, successKey, playlistName);

        _mockMessage.Verify(x => x.RespondAsync(successMessage), Times.Once);
    }

    [Fact]
    public async Task SendSuccessAsync_WhenMessageHasNoGuild_UsesDefaultLocalization()
    {
        const string successKey = "pong";
        const string successMessage = "Pong!";

        var channel = new Mock<IDiscordChannel>();
        channel.SetupGet(x => x.Guild).Returns((IDiscordGuild)null!);
        _mockMessage.SetupGet(x => x.Channel).Returns(channel.Object);

        _mockLocalizationService
            .Setup(x => x.Get(successKey))
            .Returns(successMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(successMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendSuccessAsync(_mockMessage.Object, successKey);

        _mockMessage.Verify(x => x.RespondAsync(successMessage), Times.Once);
        _mockLocalizationService.Verify(
            x => x.Get(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<object[]>()),
            Times.Never);
    }

    [Fact]
    public async Task SendSuccessAsync_EmptyLocalizationKey_SendsEmptyMessage()
    {
        const string successKey = "";

        _mockMessage
            .Setup(x => x.RespondAsync(successKey))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendSuccessAsync(_mockMessage.Object, successKey);

        _mockMessage.Verify(x => x.RespondAsync(successKey), Times.Once);
    }

    [Fact]
    public async Task SendSuccessAsync_LongLocalizedMessage_SendsEntireMessage()
    {
        const string successKey = "long_success";
        var longMessage = new string('x', 2000);

        _mockLocalizationService
            .Setup(x => x.Get(successKey))
            .Returns(longMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(longMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendSuccessAsync(_mockMessage.Object, successKey);

        _mockMessage.Verify(x => x.RespondAsync(longMessage), Times.Once);
    }

    #endregion

    #region SendWarningAsync Tests

    [Fact]
    public async Task SendWarningAsync_ValidLocalizationKey_SendsPrefixedLocalizedMessage()
    {
        const string warningPrefix = "**Warning:** ";
        const string warningKey = "playlist_already_exists";
        const string warningMessage = "Playlist already exists.";
        const string expectedMessage = warningPrefix + warningMessage;

        _mockLocalizationService
            .Setup(x => x.Get("response_warning_prefix"))
            .Returns(warningPrefix);
        _mockLocalizationService
            .Setup(x => x.Get(warningKey))
            .Returns(warningMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(expectedMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendWarningAsync(_mockMessage.Object, warningKey);

        _mockMessage.Verify(x => x.RespondAsync(expectedMessage), Times.Once);
    }

    [Fact]
    public async Task SendWarningAsync_MissingPrefix_SendsLocalizedMessageWithoutPrefix()
    {
        const string warningKey = "playlist_already_exists";
        const string warningMessage = "Playlist already exists.";

        _mockLocalizationService
            .Setup(x => x.Get(warningKey))
            .Returns(warningMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(warningMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendWarningAsync(_mockMessage.Object, warningKey);

        _mockMessage.Verify(x => x.RespondAsync(warningMessage), Times.Once);
    }

    #endregion

    #region SendErrorAsync Tests

    [Fact]
    public async Task SendErrorAsync_ValidLocalizationKey_SendsPrefixedLocalizedMessage()
    {
        const string errorPrefix = "**Error:** ";
        const string errorKey = "unknown_error";
        const string errorMessage = "Unknown error occurred.";
        const string expectedMessage = errorPrefix + errorMessage;

        _mockLocalizationService
            .Setup(x => x.Get("response_error_prefix"))
            .Returns(errorPrefix);
        _mockLocalizationService
            .Setup(x => x.Get(errorKey))
            .Returns(errorMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(expectedMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendErrorAsync(_mockMessage.Object, errorKey);

        _mockMessage.Verify(x => x.RespondAsync(expectedMessage), Times.Once);
    }

    [Fact]
    public async Task SendErrorAsync_MissingPrefix_SendsLocalizedMessageWithoutPrefix()
    {
        const string errorKey = "unknown_error";
        const string errorMessage = "Unknown error occurred.";

        _mockLocalizationService
            .Setup(x => x.Get(errorKey))
            .Returns(errorMessage);

        _mockMessage
            .Setup(x => x.RespondAsync(errorMessage))
            .Returns(Task.CompletedTask);

        await _responseBuilder.SendErrorAsync(_mockMessage.Object, errorKey);

        _mockMessage.Verify(x => x.RespondAsync(errorMessage), Times.Once);
    }

    #endregion

    private void SetupMessageGuild()
    {
        var guild = new Mock<IDiscordGuild>();
        guild.SetupGet(x => x.Id).Returns(GuildId);

        var channel = new Mock<IDiscordChannel>();
        channel.SetupGet(x => x.Guild).Returns(guild.Object);

        _mockMessage.SetupGet(x => x.Channel).Returns(channel.Object);
    }
}
