using System;
using System.IO;
using System.Linq;
using DotNetEnv;
using Lavalink4NET;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using DC_bot.Interface;
using DC_bot.Service;
using DSharpPlus;
using DC_bot.Wrapper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DC_bot_tests.IntegrationTests.ServiceTest;

public class ReactionHandlerIntegrationTests
{
    private readonly Mock<ILogger<SingletonDiscordClient>> _loggerSingletonDiscordClientMock = new();
    private readonly Mock<ILogger<LavaLinkService>> _loggerLavalinkServiceMock = new();
    private readonly Mock<ILogger<ReactionHandler>> _loggerReactionHandlerMock = new();
    private readonly Mock<ILogger<ValidationService>> _validationLoggerMock = new();
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock = new();
    private readonly Mock<IAudioService> _audioServiceMock = new();
    private readonly Mock<IResponseBuilder> _responseBuilderMock = new();
  
    private const ulong TestChannelId = 1339151008307351572;
    private readonly DiscordClient _discordClient;

    public ReactionHandlerIntegrationTests()
    {
        var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;

        var envPath = Path.Combine(directoryInfo, ".env");
        Env.Load(envPath);

        _localizationServiceMock.Setup(ls => ls.Get("skip_command_response"))
            .Returns("Now Playing: ");
        _localizationServiceMock.Setup(ls => ls.Get("music_control"))
            .Returns("Music Controls");
        
        var validationService =
            new ValidationService(_validationLoggerMock.Object, true);
        var lavalinkService = new LavaLinkService(_musicQueueServiceMock.Object,_loggerLavalinkServiceMock.Object,_audioServiceMock.Object,validationService,_responseBuilderMock.Object,_localizationServiceMock.Object);
        
        var _reactionHandlerService = new ReactionHandler(lavalinkService,
            _loggerReactionHandlerMock.Object, _localizationServiceMock.Object);

        var service = new ServiceCollection()
            .AddSingleton<IValidationService>(validationService)
            .AddSingleton(lavalinkService)
            .AddSingleton(_musicQueueServiceMock.Object)
            .AddSingleton(_localizationServiceMock.Object)
            .AddSingleton(_reactionHandlerService).BuildServiceProvider();

        ServiceLocator.SetServiceProvider(service);

        SingletonDiscordClient.InitializeLogger(_loggerSingletonDiscordClientMock.Object);
        _discordClient = SingletonDiscordClient.Instance;
    }

    [Fact]
    public void RegisterHandler_ShouldRegisterEvent()
    {
        var reactionHandler = ServiceLocator.GetService<ReactionHandler>();
        reactionHandler.RegisterHandler(_discordClient);

        _loggerReactionHandlerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registered reaction handler.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        reactionHandler.UnRegisterHandler(_discordClient);
    }

    [Fact]
    public void RegisterHandler_TryToRegisterTwice()
    {
        var reactionHandler = ServiceLocator.GetService<ReactionHandler>();
        reactionHandler.RegisterHandler(_discordClient);
        reactionHandler.RegisterHandler(_discordClient);

        _loggerReactionHandlerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ReactionHandler Service is already registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        reactionHandler.UnRegisterHandler(_discordClient);
    }

    [Fact]
    public void UnregisterCommandHandler_Should_Log_Warning()
    {
        var reactionHandler = ServiceLocator.GetService<ReactionHandler>();
        reactionHandler.UnRegisterHandler(_discordClient);

        _loggerReactionHandlerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Tried to unregister handlers, but it was not registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ReactionHandler_ShouldHandleReactionRemovedEvent()
    {
        // Arrange
        var reactionHandler = ServiceLocator.GetService<ReactionHandler>();
        reactionHandler.RegisterHandler(_discordClient);
        var lavalinkService = ServiceLocator.GetService<LavaLinkService>();

        // Set initial state for repeat mode
        var channel = await SingletonDiscordClient.Instance.GetChannelAsync(TestChannelId);
        var discordChannel = new DiscordChannelWrapper(channel);
        var track = new Mock<ILavaLinkTrack>();

        track.SetupGet(t => t.Author).Returns("Test Author");
        track.SetupGet(t => t.Title).Returns("Test Title");
        
        await lavalinkService.TrackStartedEventTrigger(discordChannel,_discordClient, track.Object);

        await Task.Delay(10000);

        var message = await channel.GetMessagesAsync(1);
        var response = message.FirstOrDefault();
        
        Assert.NotNull(response);
        Assert.NotNull(response.Reactions);
        Assert.Contains(response.Reactions, x => x.Emoji == "\u23f8\ufe0f");
        Assert.Contains(response.Reactions, x => x.Emoji == "\u25b6\ufe0f");
        Assert.Contains(response.Reactions, x => x.Emoji == "\u23ed\ufe0f");
        Assert.Contains(response.Reactions, x => x.Emoji =="\ud83d\udd01");
        Assert.Contains("🎵 **Music Controls** 🎵", response.Content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Now Playing: Test Author - Test Title", response.Content, StringComparison.OrdinalIgnoreCase);
        _loggerReactionHandlerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Reaction control message sent and reactions added.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        reactionHandler.UnRegisterHandler(_discordClient);
    }
}