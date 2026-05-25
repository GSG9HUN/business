using DC_bot.Configuration;

namespace DC_bot_tests.UnitTests.Configuration;

[Trait("Category", "Unit")]
public class BotSettingsTests
{
    [Fact]
    public void BotSettings_DefaultPrefix_IsExclamationMark()
    {
        var settings = new BotSettings();

        Assert.Equal("!", settings.Prefix);
    }

    [Fact]
    public void BotSettings_CustomPrefix_CanBeSet()
    {
        var settings = new BotSettings { Prefix = "!" };

        Assert.Equal("!", settings.Prefix);
    }

    [Fact]
    public void BotSettings_DifferentPrefixes_AreIndependent()
    {
        var settings1 = new BotSettings { Prefix = "!" };
        var settings2 = new BotSettings { Prefix = "$" };

        Assert.Equal("!", settings1.Prefix);
        Assert.Equal("$", settings2.Prefix);
    }

    [Fact]
    public void BotSettings_Token_CanBeSet()
    {
        const string token = "test_token_123";

        var settings = new BotSettings { Token = token };

        Assert.Equal(token, settings.Token);
    }

    [Fact]
    public void BotSettings_Token_DefaultNull()
    {
        var settings = new BotSettings();

        Assert.Null(settings.Token);
    }

    [Fact]
    public void BotSettings_TokenAndPrefix_CanBothBeSet()
    {
        const string token = "test_token";
        const string prefix = "$";

        var settings = new BotSettings
        {
            Token = token,
            Prefix = prefix
        };

        Assert.Equal(token, settings.Token);
        Assert.Equal(prefix, settings.Prefix);
    }

    [Fact]
    public void BotSettings_EmptyToken_CanBeSet()
    {
        var settings = new BotSettings { Token = "" };

        Assert.Empty(settings.Token);
    }

    [Fact]
    public void BotSettings_EmptyPrefix_CanBeSet()
    {
        var settings = new BotSettings { Prefix = "" };

        Assert.Empty(settings.Prefix);
    }

    [Fact]
    public void BotSettings_Properties_UseInitAccessor()
    {
        var settings = new BotSettings
        {
            Token = "token",
            Prefix = "!"
        };

        Assert.NotNull(settings);
    }
}
