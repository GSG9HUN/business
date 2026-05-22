using DC_bot.Configuration;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Wrapper;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Wrapper;

[Trait("Category", "Unit")]
public class DiscordClientFactoryTests
{
    [Fact]
    public void Create_WithValidSettings_ReturnsDiscordClient()
    {
        var settings = new BotSettings { Token = "valid_test_token" };
        var eventHandler = CreateEventHandler();

        using var client = DiscordClientFactory.Create(settings, eventHandler);

        Assert.NotNull(client);
    }

    [Fact]
    public void Create_CalledTwice_ReturnsIndependentClients()
    {
        var settings = new BotSettings { Token = "valid_test_token" };

        using var firstClient = DiscordClientFactory.Create(settings, CreateEventHandler());
        using var secondClient = DiscordClientFactory.Create(settings, CreateEventHandler());

        Assert.NotSame(firstClient, secondClient);
    }

    [Fact]
    public void Create_WhenTokenIsNull_ThrowsConfiguredException()
    {
        var settings = new BotSettings { Token = null };

        var exception = Assert.Throws<Exception>(() =>
            DiscordClientFactory.Create(settings, CreateEventHandler()));

        Assert.Equal("DISCORD_TOKEN is not set.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenTokenIsBlank_ThrowsArgumentNullException(string token)
    {
        var settings = new BotSettings { Token = token };

        var exception = Assert.Throws<ArgumentNullException>(() =>
            DiscordClientFactory.Create(settings, CreateEventHandler()));

        Assert.Equal("value", exception.ParamName);
    }

    private static DiscordClientEventHandler CreateEventHandler()
    {
        var logger = Mock.Of<ILogger<DiscordClientEventHandler>>();
        var guildDataRepository = Mock.Of<IGuildDataRepository>();
        var serviceProvider = Mock.Of<IServiceProvider>();

        return new DiscordClientEventHandler(logger, guildDataRepository, serviceProvider);
    }
}
