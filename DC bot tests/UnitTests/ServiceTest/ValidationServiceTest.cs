using DC_bot.Interface;
using DC_bot.Service;
using Lavalink4NET;
using Lavalink4NET.Players;
using Moq;
using Microsoft.Extensions.Logging;

namespace DC_bot_tests.UnitTests.ServiceTest;

public class ValidationServiceTest
{
    private readonly Mock<ILocalizationService> _localizationServiceMock;
    private readonly ValidationService _validationService;

    public ValidationServiceTest()
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

        audioServiceMock.Setup(a => a.Players.GetPlayerAsync(guildId,default))
            .ReturnsAsync((ILavalinkPlayer?)null);
        
        _localizationServiceMock.Setup(l => l.Get("lavalink_error"))
            .Returns("Lavalink is not connected.");

        // Act
        var result = await _validationService.ValidatePlayerAsync(audioServiceMock.Object, guildId);

        // Assert
        Assert.False(result.isValid);
        Assert.Null(result.player);
        Assert.Equal("lavalink_error", result.errorKey);
    }

    [Fact]
    public async Task ValidatePlayerAsync_PlayerExists_ReturnsTrue()
    {
        // Arrange
        var audioServiceMock = new Mock<IAudioService>();
        var textChannelMock = new Mock<IDiscordChannel>();
        ulong guildId = 12345;

        var mockPlayer = new Mock<ILavalinkPlayer>();
        audioServiceMock.Setup(a => a.Players.GetPlayerAsync(guildId,default))
            .ReturnsAsync(mockPlayer.Object);

        // Act
        var result = await _validationService.ValidatePlayerAsync(audioServiceMock.Object, guildId);

        // Assert
        Assert.True(result.isValid);
        Assert.NotNull(result.player);
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
        Assert.False(result.isValid);
        Assert.Null(result.connection);
        Assert.Equal("bot_is_not_connected_error",result.errorKey);
        
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

        _localizationServiceMock.Setup(l => l.Get("bot_is_not_connected_error"))
            .Returns("Bot is not connected to a voice channel.");

        // Act
        var result = await _validationService.ValidateConnectionAsync(lavalinkPlayer.Object);

        // Assert
        Assert.True(result.isValid);
        Assert.NotNull(result.connection);
        textChannelMock.Verify(t => t.SendMessageAsync("Bot is not connected to a voice channel."), Times.Never);
    }
    
}
