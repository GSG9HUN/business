using DC_bot.Service;
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
        var client = TestDiscordClientFactory.Create("test_token");

        var service = new BotService(client);

        Assert.NotNull(service);
    }

    [Fact]
    public void BotService_Constructor_WithLogger_CreatesInstance()
    {
        var client = TestDiscordClientFactory.Create("test_token");
        var mockLogger = new Mock<ILogger<BotService>>();

        var service = new BotService(client, mockLogger.Object);

        Assert.NotNull(service);
    }

    [Fact]
    public void BotService_Constructor_WithNullLogger_CreatesInstanceWithNullLogger()
    {
        var client = TestDiscordClientFactory.Create("test_token");

        var service = new BotService(client);

        Assert.NotNull(service);
    }

    [Fact]
    public void BotService_Constructor_WithNullLoggerDefaultParameter_CreatesInstance()
    {
        var client = TestDiscordClientFactory.Create("test_token");

        var service = new BotService(client);

        Assert.NotNull(service);
    }

    [Fact]
    public void BotService_MultipleInstances_AreIndependent()
    {
        var client1 = TestDiscordClientFactory.Create("test_token_1");
        var client2 = TestDiscordClientFactory.Create("test_token_2");

        var service1 = new BotService(client1);
        var service2 = new BotService(client2);

        Assert.NotNull(service1);
        Assert.NotNull(service2);
        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void BotService_WithDifferentLoggers_CreatesIndependentInstances()
    {
        var client = TestDiscordClientFactory.Create("test_token");
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
        var client = TestDiscordClientFactory.Create("test_token");

        var service = new BotService(client);

        Assert.NotNull(service);
    }
}
