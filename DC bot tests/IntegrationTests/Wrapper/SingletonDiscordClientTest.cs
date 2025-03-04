using DC_bot.Interface;
using DC_bot.Service;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.IntegrationTests.Wrapper;

public class SingletonDiscordClientTest
{
    [Fact]
    public void Instance_Should_Return_Singleton_DiscordClient()
    {
        // Act
        //var instance1 = SingletonDiscordClient.Instance;
        //var instance2 = SingletonDiscordClient.Instance;

        // Assert
        //Assert.NotNull(instance1);
        //Assert.Same(instance1, instance2); // Ugyanazt az egy példányt adja vissza
    }
    
    private readonly Mock<ILogger<SingletonDiscordClient>> _mockLogger;
        private readonly Mock<MusicQueueService> _mockMusicQueueService;
        private readonly Mock<ILavaLinkService> _mockLavaLinkService;
    

        public SingletonDiscordClientTest()
        {
            _mockLogger = new Mock<ILogger<SingletonDiscordClient>>();
            _mockMusicQueueService = new Mock<MusicQueueService>();
            _mockLavaLinkService = new Mock<ILavaLinkService>();
        }

        [Fact]
        public async Task OnGuildAvailable_ShouldInitializeMusicServiceAndLavaLinkService()
        {
            // Arrange
            var guildId = 12345UL; // Sample Guild ID
            var discordClientMock = new Mock<DiscordClient>(MockBehavior.Loose, new DiscordConfiguration
            {
                Token = "TestToken",
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged,
                AutoReconnect = true
            });

            // Setup ServiceLocator mocks
            var services = new ServiceCollection()
                .AddSingleton(_mockMusicQueueService.Object)
                .AddSingleton(_mockLavaLinkService.Object)
                .BuildServiceProvider();
            
            ServiceLocator.SetServiceProvider(services);

            SingletonDiscordClient.InitializeLogger(_mockLogger.Object);

            // Act
           

            // Assert
            _mockLogger.Verify(logger => logger.LogInformation(It.Is<string>(s => s.Contains("Guild available"))), Times.Once);
            _mockMusicQueueService.Verify(service => service.Init(guildId), Times.Once);
            _mockLavaLinkService.Verify(service => service.Init(guildId), Times.Once);

            // Verify connection setup
            _mockLavaLinkService.Verify(service => service.ConnectAsync(), Times.Once);
        }

        [Fact]
        public void ShouldInitializeLogger_WhenInitializeLoggerIsCalled()
        {
            // Act
            SingletonDiscordClient.InitializeLogger(_mockLogger.Object);

            // Assert
            _mockLogger.Verify(logger => logger.LogInformation(It.Is<string>(s => s.Contains("Logger initialized"))), Times.Once);
        }
}