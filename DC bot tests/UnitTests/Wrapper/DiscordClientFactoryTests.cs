using DC_bot.Configuration;
using DC_bot.Wrapper;

namespace DC_bot_tests.UnitTests.Wrapper;

[Trait("Category", "Unit")]
public class DiscordClientFactoryTests
{
    [Fact]
    public void Create_WithValidSettings_ReturnsDiscordClient()
    {
        var settings = new BotSettings { Token = "valid_test_token" };

        var client = DiscordClientFactory.Create(settings);

        Assert.NotNull(client);
    }

    [Fact]
    public void Create_CalledTwice_ReturnsIndependentClients()
    {
        var settings = new BotSettings { Token = "valid_test_token" };

        var firstClient = DiscordClientFactory.Create(settings);
        var secondClient = DiscordClientFactory.Create(settings);

        Assert.NotSame(firstClient, secondClient);
    }

    [Fact]
    public void Create_WhenTokenIsNull_ThrowsConfiguredException()
    {
        var settings = new BotSettings { Token = null };

        var exception = Assert.Throws<Exception>(() =>
            DiscordClientFactory.Create(settings));

        Assert.Equal("DISCORD_TOKEN is not set.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenTokenIsBlank_ThrowsArgumentNullException(string token)
    {
        var settings = new BotSettings { Token = token };

        var exception = Assert.Throws<ArgumentNullException>(() =>
            DiscordClientFactory.Create(settings));

        Assert.Equal("value", exception.ParamName);
    }
}
