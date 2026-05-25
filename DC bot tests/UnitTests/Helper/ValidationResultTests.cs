using DC_bot.Helper.Validation;
using DC_bot.Interface.Discord;
using Lavalink4NET.Players;
using Moq;

namespace DC_bot_tests.UnitTests.Helper;

[Trait("Category", "Unit")]
public class ValidationResultTests
{
    #region UserValidationResult Tests

    [Fact]
    public void UserValidationResult_ValidResult_HasCorrectProperties()
    {
        var mockMember = new Mock<IDiscordMember>();

        var result = new UserValidationResult(true, string.Empty, mockMember.Object);

        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorKey);
        Assert.Equal(mockMember.Object, result.Member);
    }

    [Fact]
    public void UserValidationResult_InvalidResult_HasErrorKey()
    {
        const string errorKey = "user_not_in_voice_channel";

        var result = new UserValidationResult(false, errorKey);

        Assert.False(result.IsValid);
        Assert.Equal(errorKey, result.ErrorKey);
        Assert.Null(result.Member);
    }

    [Fact]
    public void UserValidationResult_DefaultMember_IsNull()
    {
        var result = new UserValidationResult(false, "error");

        Assert.Null(result.Member);
    }

    [Fact]
    public void UserValidationResult_ValidWithoutMember_CreatesSuccessfully()
    {
        var result = new UserValidationResult(true, string.Empty);

        Assert.True(result.IsValid);
        Assert.Null(result.Member);
    }

    #endregion

    #region PlayerValidationResult Tests

    [Fact]
    public void PlayerValidationResult_ValidResult_HasCorrectProperties()
    {
        var mockPlayer = new Mock<ILavalinkPlayer>();

        var result = new PlayerValidationResult(true, string.Empty, mockPlayer.Object);

        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorKey);
        Assert.Equal(mockPlayer.Object, result.Player);
    }

    [Fact]
    public void PlayerValidationResult_InvalidResult_HasErrorKey()
    {
        const string errorKey = "lavalink_error";

        var result = new PlayerValidationResult(false, errorKey, null);

        Assert.False(result.IsValid);
        Assert.Equal(errorKey, result.ErrorKey);
        Assert.Null(result.Player);
    }

    [Fact]
    public void PlayerValidationResult_DefaultPlayer_IsNull()
    {
        var result = new PlayerValidationResult(false, "error", null);

        Assert.Null(result.Player);
    }

    [Fact]
    public void PlayerValidationResult_ValidWithoutPlayer_CreatesSuccessfully()
    {
        var result = new PlayerValidationResult(true, string.Empty, null);

        Assert.True(result.IsValid);
        Assert.Null(result.Player);
    }

    #endregion

    #region ConnectionValidationResult Tests

    [Fact]
    public void ConnectionValidationResult_ValidResult_HasCorrectProperties()
    {
        var mockConnection = new Mock<ILavalinkPlayer>();

        var result = new ConnectionValidationResult(true, string.Empty, mockConnection.Object);

        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorKey);
        Assert.Equal(mockConnection.Object, result.Connection);
    }

    [Fact]
    public void ConnectionValidationResult_InvalidResult_HasErrorKey()
    {
        const string errorKey = "bot_not_connected";

        var result = new ConnectionValidationResult(false, errorKey, null);

        Assert.False(result.IsValid);
        Assert.Equal(errorKey, result.ErrorKey);
        Assert.Null(result.Connection);
    }

    [Fact]
    public void ConnectionValidationResult_DefaultConnection_IsNull()
    {
        var result = new ConnectionValidationResult(false, "error", null);

        Assert.Null(result.Connection);
    }

    [Fact]
    public void ConnectionValidationResult_ValidWithoutConnection_CreatesSuccessfully()
    {
        var result = new ConnectionValidationResult(true, string.Empty, null);

        Assert.True(result.IsValid);
        Assert.Null(result.Connection);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ValidationResults_MultipleInstances_AreIndependent()
    {
        var mockMember = new Mock<IDiscordMember>();
        var mockPlayer = new Mock<ILavalinkPlayer>();

        var userResult = new UserValidationResult(true, string.Empty, mockMember.Object);
        var playerResult = new PlayerValidationResult(true, string.Empty, mockPlayer.Object);

        Assert.NotNull(userResult.Member);
        Assert.NotNull(playerResult.Player);
    }

    [Fact]
    public void ValidationResults_ErrorKeysAreDifferent_StayIndependent()
    {
        const string errorKey1 = "error_1";
        const string errorKey2 = "error_2";

        var result1 = new UserValidationResult(false, errorKey1);
        var result2 = new PlayerValidationResult(false, errorKey2, null);

        Assert.Equal(errorKey1, result1.ErrorKey);
        Assert.Equal(errorKey2, result2.ErrorKey);
        Assert.NotEqual(result1.ErrorKey, result2.ErrorKey);
    }

    [Fact]
    public void ValidationResults_ValidityFlags_CanBeCombined()
    {
        var validUser = new UserValidationResult(true, string.Empty);
        var invalidPlayer = new PlayerValidationResult(false, "error", null);
        var validConnection = new ConnectionValidationResult(true, string.Empty, null);

        Assert.True(validUser.IsValid);
        Assert.False(invalidPlayer.IsValid);
        Assert.True(validConnection.IsValid);
    }

    #endregion
}
