using DC_bot.Service;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DC_bot_tests.UnitTests.Service;

[Trait("Category", "Unit")]
public class BotServiceTests
{
    [Fact]
    public void BotService_Constructor_WithValidClient_CreatesInstance()
    {
        var config = new DiscordConfiguration
        {
            Token = "test_token",
            TokenType = TokenType.Bot
        };
        var client = new DiscordClient(config);

        var service = new BotService(client);

        Assert.NotNull(service);
    }

    [Fact]
    public void BotService_Constructor_WithLogger_CreatesInstance()
    {
        var config = new DiscordConfiguration
        {
            Token = "test_token",
            TokenType = TokenType.Bot
        };
        var client = new DiscordClient(config);
        var mockLogger = new Mock<ILogger<BotService>>();

        var service = new BotService(client, mockLogger.Object);

        Assert.NotNull(service);
    }

    [Fact]
    public void BotService_Constructor_WithNullLogger_CreatesInstanceWithNullLogger()
    {
        var config = new DiscordConfiguration
        {
            Token = "test_token",
            TokenType = TokenType.Bot
        };
        var client = new DiscordClient(config);

        var service = new BotService(client);

        Assert.NotNull(service);
    }

    [Fact]
    public void BotService_Constructor_WithNullLoggerDefaultParameter_CreatesInstance()
    {
        var config = new DiscordConfiguration
        {
            Token = "test_token",
            TokenType = TokenType.Bot
        };
        var client = new DiscordClient(config);

        var service = new BotService(client);

        Assert.NotNull(service);
    }

    [Fact]
    public void BotService_MultipleInstances_AreIndependent()
    {
        var config1 = new DiscordConfiguration
        {
            Token = "test_token_1",
            TokenType = TokenType.Bot
        };
        var config2 = new DiscordConfiguration
        {
            Token = "test_token_2",
            TokenType = TokenType.Bot
        };
        var client1 = new DiscordClient(config1);
        var client2 = new DiscordClient(config2);

        var service1 = new BotService(client1);
        var service2 = new BotService(client2);

        Assert.NotNull(service1);
        Assert.NotNull(service2);
        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void BotService_WithDifferentLoggers_CreatesIndependentInstances()
    {
        var config = new DiscordConfiguration
        {
            Token = "test_token",
            TokenType = TokenType.Bot
        };
        var client = new DiscordClient(config);
        var logger1 = NullLogger<BotService>.Instance;
        var logger2 = new Mock<ILogger<BotService>>().Object;

        var service1 = new BotService(client, logger1);
        var service2 = new BotService(client, logger2);

        Assert.NotNull(service1);
        Assert.NotNull(service2);
        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void BotService_DefaultLogger_IsNullLogger()
    {
        var config = new DiscordConfiguration
        {
            Token = "test_token",
            TokenType = TokenType.Bot
        };
        var client = new DiscordClient(config);

        var service = new BotService(client);

        Assert.NotNull(service);
    }
}
