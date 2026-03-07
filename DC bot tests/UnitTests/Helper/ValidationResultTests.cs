using DC_bot.Helper.Validation;
using DC_bot.Interface.Discord;
using Lavalink4NET.Players;
using Moq;

namespace DC_bot_tests.UnitTests.Helper;

public class ValidationResultTests
{
    #region UserValidationResult Tests

    [Fact]
    public void UserValidationResult_ValidResult_HasCorrectProperties()
    {
        // Arrange
        var mockMember = new Mock<IDiscordMember>();

        // Act
        var result = new UserValidationResult(true, string.Empty, mockMember.Object);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorKey);
        Assert.Equal(mockMember.Object, result.Member);
    }

    [Fact]
    public void UserValidationResult_InvalidResult_HasErrorKey()
    {
        // Arrange
        const string errorKey = "user_not_in_voice_channel";

        // Act
        var result = new UserValidationResult(false, errorKey);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(errorKey, result.ErrorKey);
        Assert.Null(result.Member);
    }

    [Fact]
    public void UserValidationResult_DefaultMember_IsNull()
    {
        // Arrange & Act
        var result = new UserValidationResult(false, "error");

        // Assert
        Assert.Null(result.Member);
    }

    [Fact]
    public void UserValidationResult_ValidWithoutMember_CreatesSuccessfully()
    {
        // Arrange & Act
        var result = new UserValidationResult(true, string.Empty);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.Member);
    }

    #endregion

    #region PlayerValidationResult Tests

    [Fact]
    public void PlayerValidationResult_ValidResult_HasCorrectProperties()
    {
        // Arrange
        var mockPlayer = new Mock<ILavalinkPlayer>();

        // Act
        var result = new PlayerValidationResult(true, string.Empty, mockPlayer.Object);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorKey);
        Assert.Equal(mockPlayer.Object, result.Player);
    }

    [Fact]
    public void PlayerValidationResult_InvalidResult_HasErrorKey()
    {
        // Arrange
        const string errorKey = "lavalink_error";

        // Act
        var result = new PlayerValidationResult(false, errorKey, null);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(errorKey, result.ErrorKey);
        Assert.Null(result.Player);
    }

    [Fact]
    public void PlayerValidationResult_DefaultPlayer_IsNull()
    {
        // Arrange & Act
        var result = new PlayerValidationResult(false, "error", null);

        // Assert
        Assert.Null(result.Player);
    }

    [Fact]
    public void PlayerValidationResult_ValidWithoutPlayer_CreatesSuccessfully()
    {
        // Arrange & Act
        var result = new PlayerValidationResult(true, string.Empty, null);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.Player);
    }

    #endregion

    #region ConnectionValidationResult Tests

    [Fact]
    public void ConnectionValidationResult_ValidResult_HasCorrectProperties()
    {
        // Arrange
        var mockConnection = new Mock<ILavalinkPlayer>();

        // Act
        var result = new ConnectionValidationResult(true, string.Empty, mockConnection.Object);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorKey);
        Assert.Equal(mockConnection.Object, result.Connection);
    }

    [Fact]
    public void ConnectionValidationResult_InvalidResult_HasErrorKey()
    {
        // Arrange
        const string errorKey = "bot_not_connected";

        // Act
        var result = new ConnectionValidationResult(false, errorKey, null);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(errorKey, result.ErrorKey);
        Assert.Null(result.Connection);
    }

    [Fact]
    public void ConnectionValidationResult_DefaultConnection_IsNull()
    {
        // Arrange & Act
        var result = new ConnectionValidationResult(false, "error", null);

        // Assert
        Assert.Null(result.Connection);
    }

    [Fact]
    public void ConnectionValidationResult_ValidWithoutConnection_CreatesSuccessfully()
    {
        // Arrange & Act
        var result = new ConnectionValidationResult(true, string.Empty, null);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.Connection);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ValidationResults_MultipleInstances_AreIndependent()
    {
        // Arrange
        var mockMember = new Mock<IDiscordMember>();
        var mockPlayer = new Mock<ILavalinkPlayer>();

        // Act
        var userResult = new UserValidationResult(true, string.Empty, mockMember.Object);
        var playerResult = new PlayerValidationResult(true, string.Empty, mockPlayer.Object);

        // Assert
        Assert.NotNull(userResult.Member);
        Assert.NotNull(playerResult.Player);
    }

    [Fact]
    public void ValidationResults_ErrorKeysAreDifferent_StayIndependent()
    {
        // Arrange
        const string errorKey1 = "error_1";
        const string errorKey2 = "error_2";

        // Act
        var result1 = new UserValidationResult(false, errorKey1);
        var result2 = new PlayerValidationResult(false, errorKey2, null);

        // Assert
        Assert.Equal(errorKey1, result1.ErrorKey);
        Assert.Equal(errorKey2, result2.ErrorKey);
        Assert.NotEqual(result1.ErrorKey, result2.ErrorKey);
    }

    [Fact]
    public void ValidationResults_ValidityFlags_CanBeCombined()
    {
        // Arrange & Act
        var validUser = new UserValidationResult(true, string.Empty);
        var invalidPlayer = new PlayerValidationResult(false, "error", null);
        var validConnection = new ConnectionValidationResult(true, string.Empty, null);

        // Assert
        Assert.True(validUser.IsValid);
        Assert.False(invalidPlayer.IsValid);
        Assert.True(validConnection.IsValid);
    }

    #endregion
}