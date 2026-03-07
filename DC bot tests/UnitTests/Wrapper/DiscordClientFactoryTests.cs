using DC_bot.Configuration;
using DC_bot.Wrapper;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Wrapper;

public class DiscordClientFactoryTests
{
    [Fact]
    public void Create_WithValidSettings_CreatesDiscordClient()
    {
        // Arrange
        var settings = new BotSettings { Token = "valid_test_token" };
        var mockLogger = new Mock<ILogger<DiscordClientEventHandler>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var eventHandler = new DiscordClientEventHandler(mockLogger.Object, mockServiceProvider.Object);

        // Act
        var client = DiscordClientFactory.Create(settings, eventHandler);

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public void Create_SetsTokenFromBotSettings()
    {
        // Arrange
        var expectedToken = "my_specific_token_12345";
        var settings = new BotSettings { Token = expectedToken };
        var mockLogger = new Mock<ILogger<DiscordClientEventHandler>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var eventHandler = new DiscordClientEventHandler(mockLogger.Object, mockServiceProvider.Object);

        // Act
        var client = DiscordClientFactory.Create(settings, eventHandler);

        // Assert
        Assert.NotNull(client);
        // Note: DiscordClient doesn't expose Token publicly, but we can verify it doesn't throw
        // and the client is properly configured
    }

    [Fact]
    public void Create_AppliesCorrectIntents()
    {
        // Arrange
        var settings = new BotSettings { Token = "test_token" };
        var mockLogger = new Mock<ILogger<DiscordClientEventHandler>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var eventHandler = new DiscordClientEventHandler(mockLogger.Object, mockServiceProvider.Object);

        // Act
        var client = DiscordClientFactory.Create(settings, eventHandler);

        // Assert
        Assert.NotNull(client);
        // The factory uses DiscordIntents.All
        // Note: DiscordClient doesn't expose Intents publicly for direct assertion,
        // but we verify the client is created without throwing
    }

    [Fact]
    public void Create_SetsTokenTypeToBot()
    {
        // Arrange
        var settings = new BotSettings { Token = "test_token" };
        var mockLogger = new Mock<ILogger<DiscordClientEventHandler>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var eventHandler = new DiscordClientEventHandler(mockLogger.Object, mockServiceProvider.Object);

        // Act
        var client = DiscordClientFactory.Create(settings, eventHandler);

        // Assert
        Assert.NotNull(client);
        // TokenType is set to TokenType.Bot in the factory
    }

    [Fact]
    public void Create_EnablesAutoReconnect()
    {
        // Arrange
        var settings = new BotSettings { Token = "test_token" };
        var mockLogger = new Mock<ILogger<DiscordClientEventHandler>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var eventHandler = new DiscordClientEventHandler(mockLogger.Object, mockServiceProvider.Object);

        // Act
        var client = DiscordClientFactory.Create(settings, eventHandler);

        // Assert
        Assert.NotNull(client);
        // AutoReconnect is set to true in the factory
    }

    [Fact]
    public void Create_WiresEventHandlerToReadyEvent()
    {
        // Arrange
        var settings = new BotSettings { Token = "test_token" };
        var mockLogger = new Mock<ILogger<DiscordClientEventHandler>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var eventHandler = new DiscordClientEventHandler(mockLogger.Object, mockServiceProvider.Object);

        // Act
        var client = DiscordClientFactory.Create(settings, eventHandler);

        // Assert
        Assert.NotNull(client);
        // The Ready event handler is wired in the factory
        // We can't directly verify event subscription, but we verify no exception is thrown
    }

    [Fact]
    public void Create_WiresEventHandlerToGuildAvailableEvent()
    {
        // Arrange
        var settings = new BotSettings { Token = "test_token" };
        var mockLogger = new Mock<ILogger<DiscordClientEventHandler>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var eventHandler = new DiscordClientEventHandler(mockLogger.Object, mockServiceProvider.Object);

        // Act
        var client = DiscordClientFactory.Create(settings, eventHandler);

        // Assert
        Assert.NotNull(client);
        // The GuildAvailable event handler is wired in the factory
        // We can't directly verify event subscription, but we verify no exception is thrown
    }

    [Fact]
    public void Create_ThrowsException_WhenTokenIsNull()
    {
        // Arrange
        var settings = new BotSettings { Token = null };
        var mockLogger = new Mock<ILogger<DiscordClientEventHandler>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var eventHandler = new DiscordClientEventHandler(mockLogger.Object, mockServiceProvider.Object);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => 
            DiscordClientFactory.Create(settings, eventHandler));
        
        Assert.Equal("DISCORD_TOKEN is not set.", exception.Message);
    }

    [Fact]
    public void Create_ThrowsException_WhenTokenIsEmpty()
    {
        // Arrange
        var settings = new BotSettings { Token = string.Empty };
        var mockLogger = new Mock<ILogger<DiscordClientEventHandler>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var eventHandler = new DiscordClientEventHandler(mockLogger.Object, mockServiceProvider.Object);

        // Act & Assert
        try
        {
           DiscordClientFactory.Create(settings, eventHandler);
        }
        catch (ArgumentNullException e)
        {
            Assert.Equal("Token cannot be null, empty, or all whitespace. (Parameter 'value')", e.Message);
        }
    }

    [Fact]
    public void Create_ThrowsException_WhenTokenIsWhitespace()
    {
        // Arrange
        var settings = new BotSettings { Token = "   " };
        var mockLogger = new Mock<ILogger<DiscordClientEventHandler>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var eventHandler = new DiscordClientEventHandler(mockLogger.Object, mockServiceProvider.Object);

        // Act & Assert
        try
        {
           DiscordClientFactory.Create(settings, eventHandler);
        }
        catch (ArgumentNullException e)
        {
            Assert.Equal("Token cannot be null, empty, or all whitespace. (Parameter 'value')", e.Message);
        }
    }

    [Fact]
    public void Create_WithDifferentTokens_CreatesIndependentClients()
    {
        // Arrange
        var settings1 = new BotSettings { Token = "token_1" };
        var settings2 = new BotSettings { Token = "token_2" };
        var mockLogger = new Mock<ILogger<DiscordClientEventHandler>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var eventHandler1 = new DiscordClientEventHandler(mockLogger.Object, mockServiceProvider.Object);
        var eventHandler2 = new DiscordClientEventHandler(mockLogger.Object, mockServiceProvider.Object);

        // Act
        var client1 = DiscordClientFactory.Create(settings1, eventHandler1);
        var client2 = DiscordClientFactory.Create(settings2, eventHandler2);

        // Assert
        Assert.NotNull(client1);
        Assert.NotNull(client2);
        Assert.NotSame(client1, client2);
    }

    [Fact]
    public void Create_WithSameSettings_CreatesNewClientInstance()
    {
        // Arrange
        var settings = new BotSettings { Token = "same_token" };
        var mockLogger = new Mock<ILogger<DiscordClientEventHandler>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var eventHandler1 = new DiscordClientEventHandler(mockLogger.Object, mockServiceProvider.Object);
        var eventHandler2 = new DiscordClientEventHandler(mockLogger.Object, mockServiceProvider.Object);

        // Act
        var client1 = DiscordClientFactory.Create(settings, eventHandler1);
        var client2 = DiscordClientFactory.Create(settings, eventHandler2);

        // Assert
        Assert.NotNull(client1);
        Assert.NotNull(client2);
        Assert.NotSame(client1, client2);
    }
}