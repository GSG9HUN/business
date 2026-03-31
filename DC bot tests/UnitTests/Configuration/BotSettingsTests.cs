using DC_bot.Configuration;

namespace DC_bot_tests.UnitTests.Configuration;

public class BotSettingsTests
{
    [Fact]
    public void BotSettings_DefaultPrefix_IsExclamationMark()
    {
        // Arrange & Act
        var settings = new BotSettings();

        // Assert
        Assert.Equal("!", settings.Prefix);
    }

    [Fact]
    public void BotSettings_CustomPrefix_CanBeSet()
    {
        // Arrange & Act
        var settings = new BotSettings { Prefix = "!" };

        // Assert
        Assert.Equal("!", settings.Prefix);
    }

    [Fact]
    public void BotSettings_DifferentPrefixes_AreIndependent()
    {
        // Arrange & Act
        var settings1 = new BotSettings { Prefix = "!" };
        var settings2 = new BotSettings { Prefix = "$" };

        // Assert
        Assert.Equal("!", settings1.Prefix);
        Assert.Equal("$", settings2.Prefix);
    }

    [Fact]
    public void BotSettings_Token_CanBeSet()
    {
        // Arrange
        const string token = "test_token_123";

        // Act
        var settings = new BotSettings { Token = token };

        // Assert
        Assert.Equal(token, settings.Token);
    }

    [Fact]
    public void BotSettings_Token_DefaultNull()
    {
        // Arrange & Act
        var settings = new BotSettings();

        // Assert
        Assert.Null(settings.Token);
    }

    [Fact]
    public void BotSettings_TokenAndPrefix_CanBothBeSet()
    {
        // Arrange
        const string token = "test_token";
        const string prefix = "$";

        // Act
        var settings = new BotSettings
        {
            Token = token,
            Prefix = prefix
        };

        // Assert
        Assert.Equal(token, settings.Token);
        Assert.Equal(prefix, settings.Prefix);
    }

    [Fact]
    public void BotSettings_EmptyToken_CanBeSet()
    {
        // Arrange & Act
        var settings = new BotSettings { Token = "" };

        // Assert
        Assert.Empty(settings.Token);
    }

    [Fact]
    public void BotSettings_EmptyPrefix_CanBeSet()
    {
        // Arrange & Act
        var settings = new BotSettings { Prefix = "" };

        // Assert
        Assert.Empty(settings.Prefix);
    }

    [Fact]
    public void BotSettings_Properties_UseInitAccessor()
    {
        // Act
        var settings = new BotSettings
        {
            Token = "token",
            Prefix = "!"
        };

        // Assert
        Assert.NotNull(settings);
    }
}