using DC_bot.Interface;
using DC_bot.Service;
using DC_bot.Wrapper;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.IntegrationTests.Wrapper;

public class SingletonDiscordClientTest
{
    private readonly Mock<ILogger<SingletonDiscordClient>> _loggerMock = new();
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock = new();
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock = new();
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();

    [Fact]
    public void Instance_Should_Return_Singleton_DiscordClient()
    {
        //Arrange
        Environment.SetEnvironmentVariable("DISCORD_TOKEN", "fake-test-token");

        // Act
        var instance1 = SingletonDiscordClient.Instance;
        var instance2 = SingletonDiscordClient.Instance;

        // Assert
        Assert.NotNull(instance1);
        Assert.Same(instance1, instance2); // Ugyanazt az egy példányt adja vissza
    }

    [Fact]
    public async Task OnGuildAvailable_ShouldInitializeMusicService_LavaLinkService_And_LocalizationService()
    {
        // Arrange

        var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;

        var envPath = Path.Combine(directoryInfo, ".env");
        Env.Load(envPath);

        // Setup ServiceLocator mocks
        var services = new ServiceCollection()
            .AddSingleton(_musicQueueServiceMock.Object)
            .AddSingleton(_lavaLinkServiceMock.Object)
            .AddSingleton(_localizationServiceMock.Object)
            .BuildServiceProvider();

        ServiceLocator.SetServiceProvider(services);
        SingletonDiscordClient.InitializeLogger(_loggerMock.Object);

        var discordClientMock = SingletonDiscordClient.Instance;

        await discordClientMock.ConnectAsync();

        await Task.Delay(5000);

        // Assert

        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Bot is ready!")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Guild available: ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.AtLeastOnce
        );

        _musicQueueServiceMock.Verify(service => service.Init(It.IsAny<ulong>()), Times.AtLeastOnce);
        _lavaLinkServiceMock.Verify(service => service.Init(It.IsAny<ulong>()), Times.AtLeastOnce);

        // Verify connection setup
        _lavaLinkServiceMock.Verify(service => service.ConnectAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public void ShouldInitializeLogger_WhenInitializeLoggerIsCalled()
    {
        // Act
        SingletonDiscordClient.InitializeLogger(_loggerMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Logger initialized for SingletonDiscordClient.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }
}