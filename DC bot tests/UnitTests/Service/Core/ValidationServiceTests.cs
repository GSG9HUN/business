using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Service.Core;
using Lavalink4NET;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Core;

public class ValidationServiceTests
{
    private readonly Mock<ILocalizationService> _localizationServiceMock;
    private readonly ValidationService _validationService;

    public ValidationServiceTests()
    {
        _localizationServiceMock = new Mock<ILocalizationService>();
        Mock<ILogger<ValidationService>> loggerMock = new();
        _validationService = new ValidationService(loggerMock.Object);
    }

    [Fact]
    public async Task ValidatePlayerAsync_PlayerIsNull_ReturnsFalseAndErrorKey()
    {
        // Arrange
        var audioServiceMock = new Mock<IAudioService>();
        ulong guildId = 12345;

        audioServiceMock.Setup(a => a.Players.GetPlayerAsync(guildId, default))
            .ReturnsAsync((ILavalinkPlayer?)null);

        _localizationServiceMock.Setup(l => l.Get(ValidationErrorKeys.LavalinkError))
            .Returns("Lavalink is not connected.");

        // Act
        var result = await _validationService.ValidatePlayerAsync(audioServiceMock.Object, guildId);

        // Assert
        Assert.False(result.IsValid);
        Assert.Null(result.Player);
        Assert.Equal(ValidationErrorKeys.LavalinkError, result.ErrorKey);
    }

    [Fact]
    public async Task ValidatePlayerAsync_PlayerExists_ReturnsTrue()
    {
        // Arrange
        var audioServiceMock = new Mock<IAudioService>();
        ulong guildId = 12345;

        var mockPlayer = new Mock<ILavalinkPlayer>();
        audioServiceMock.Setup(a => a.Players.GetPlayerAsync(guildId, default))
            .ReturnsAsync(mockPlayer.Object);

        // Act
        var result = await _validationService.ValidatePlayerAsync(audioServiceMock.Object, guildId);

        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.Player);
    }

    [Fact]
    public async Task ValidateConnectionAsync_ConnectionIsNull_ReturnsFalseAndErrorKey()
    {
        // Arrange
        var lavalinkPlayer = new Mock<ILavalinkPlayer>();
        var playerConnectionState = new PlayerConnectionState
        {
            IsConnected = false
        };

        lavalinkPlayer.SetupGet(l => l.ConnectionState).Returns(playerConnectionState);

        // Act
        var result = await _validationService.ValidateConnectionAsync(lavalinkPlayer.Object);

        // Assert
        Assert.False(result.IsValid);
        Assert.Null(result.Connection);
        Assert.Equal(ValidationErrorKeys.BotIsNotConnectedError, result.ErrorKey);
    }

    [Fact]
    public async Task ValidateConnectionAsync_ConnectionIsExists_ReturnsTrue()
    {
        // Arrange
        var textChannelMock = new Mock<IDiscordChannel>();
        var lavalinkPlayer = new Mock<ILavalinkPlayer>();
        var playerConnectionState = new PlayerConnectionState
        {
            IsConnected = true
        };

        lavalinkPlayer.SetupGet(l => l.ConnectionState).Returns(playerConnectionState);

        textChannelMock.Setup(t => t.SendMessageAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _localizationServiceMock.Setup(l => l.Get(ValidationErrorKeys.BotIsNotConnectedError))
            .Returns("Bot is not connected to a voice channel.");

        // Act
        var result = await _validationService.ValidateConnectionAsync(lavalinkPlayer.Object);

        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.Connection);
        textChannelMock.Verify(t => t.SendMessageAsync("Bot is not connected to a voice channel."), Times.Never);
    }

    [Fact]
    public async Task ValidateUserAsync_UserIsBot_ShouldReturnInvalidWithoutErrorKey()
    {
        // Arrange
        var messageMock = new Mock<IDiscordMessage>();
        var authorMock = new Mock<IDiscordUser>();

        authorMock.SetupGet(a => a.IsBot).Returns(true);
        messageMock.SetupGet(m => m.Author).Returns(authorMock.Object);

        // Act
        var result = await _validationService.ValidateUserAsync(messageMock.Object);

        // Assert
        Assert.False(result.IsValid);
        Assert.Empty(result.ErrorKey);
    }

    [Fact]
    public async Task ValidateUserAsync_UserNotInVoiceChannel_ShouldReturnInvalidWithErrorKey()
    {
        // Arrange
        var messageMock = new Mock<IDiscordMessage>();
        var authorMock = new Mock<IDiscordUser>();
        var guildMock = new Mock<IDiscordGuild>();
        var memberMock = new Mock<IDiscordMember>();
        var voiceState = new Mock<IDiscordVoiceState>();

        authorMock.SetupGet(a => a.Id).Returns(12345);
        authorMock.SetupGet(a => a.IsBot).Returns(false);
        messageMock.SetupGet(m => m.Author).Returns(authorMock.Object);
        messageMock.SetupGet(m => m.Channel.Guild).Returns(guildMock.Object);
        voiceState.Setup(vs => vs.Channel).Returns((IDiscordChannel?)null);
        memberMock.SetupGet(m => m.VoiceState).Returns(voiceState.Object);
        guildMock.Setup(g => g.GetMemberAsync(12345)).ReturnsAsync(memberMock.Object);

        // Act
        var result = await _validationService.ValidateUserAsync(messageMock.Object);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(ValidationErrorKeys.UserNotInVoiceChannel, result.ErrorKey);
    }

    [Fact]
    public async Task ValidateUserAsync_UserInVoiceChannel_ShouldReturnValid()
    {
        // Arrange
        var messageMock = new Mock<IDiscordMessage>();
        var authorMock = new Mock<IDiscordUser>();
        var guildMock = new Mock<IDiscordGuild>();
        var discordMemberMock = new Mock<IDiscordMember>();
        var voiceStateMock = new Mock<IDiscordVoiceState>();
        var channelMock = new Mock<IDiscordChannel>();

        authorMock.SetupGet(a => a.Id).Returns(12345);
        authorMock.SetupGet(a => a.IsBot).Returns(false);
        messageMock.SetupGet(m => m.Author).Returns(authorMock.Object);
        messageMock.SetupGet(m => m.Channel.Guild).Returns(guildMock.Object);
        guildMock.Setup(g => g.GetMemberAsync(12345)).ReturnsAsync(discordMemberMock.Object);
        voiceStateMock.SetupGet(vs => vs.Channel).Returns(channelMock.Object);
        discordMemberMock.Setup(d => d.VoiceState).Returns(voiceStateMock.Object);

        // Act
        var result = await _validationService.ValidateUserAsync(messageMock.Object);

        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.Member);
        Assert.Equal(discordMemberMock.Object, result.Member);
    }

    [Fact]
    public void IsBotUser_BotMessage_ShouldReturnTrue()
    {
        // Arrange
        var messageMock = new Mock<IDiscordMessage>();
        var authorMock = new Mock<IDiscordUser>();

        authorMock.SetupGet(a => a.IsBot).Returns(true);
        messageMock.SetupGet(m => m.Author).Returns(authorMock.Object);

        // Act
        var result = _validationService.IsBotUser(messageMock.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsBotUser_HumanMessage_ShouldReturnFalse()
    {
        // Arrange
        var messageMock = new Mock<IDiscordMessage>();
        var authorMock = new Mock<IDiscordUser>();

        authorMock.SetupGet(a => a.IsBot).Returns(false);
        messageMock.SetupGet(m => m.Author).Returns(authorMock.Object);

        // Act
        var result = _validationService.IsBotUser(messageMock.Object);

        // Assert
        Assert.False(result);
    }
}