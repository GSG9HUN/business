using DC_bot.Commands.Utility;
using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using DC_bot.Service.Presentation;
using DC_bot.Wrapper;
using DotNetEnv;
using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service.Core;

[Collection("Integration Tests")]
public class CommandHandlerServiceTests : IAsyncLifetime
{
    private const string BotPrefix = "!";
    private readonly Mock<ILogger<CommandHandlerService>> _commandServiceLoggerMock = new();
    private readonly Mock<ILogger<ValidationService>> _validationLoggerMock = new();
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock = new();
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock = new();
    private const ulong TestChannelId = 1339151008307351572;
    private readonly DiscordClient _discordClient;
    private readonly CommandHandlerService _commandHandlerService;
    private readonly ServiceProvider _serviceProvider;

    public CommandHandlerServiceTests()
    {
        var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName ?? "";

        var envPath = Path.Combine(directoryInfo, ".env");
        Env.Load(envPath);
        
        var envToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        var botSettings = new BotSettings { Token = string.IsNullOrWhiteSpace(envToken) ? "fake-test-token" : envToken, Prefix = BotPrefix };

        _localizationServiceMock.Setup(ls => ls.Get(LocalizationKeys.UnknownCommandError))
            .Returns("Unknown command. Use `!help` to see available commands.");
        var userValidationService = new ValidationService(_validationLoggerMock.Object, true);

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(_localizationServiceMock.Object)
            .AddSingleton<ICommand, PingCommand>()
            .AddSingleton<IResponseBuilder, ResponseBuilder>()
            .AddSingleton<IUserValidationService>(userValidationService)
            .BuildServiceProvider();

        _commandHandlerService = new CommandHandlerService(services, _commandServiceLoggerMock.Object,
            _localizationServiceMock.Object, botSettings, true);

        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(botSettings)
            .AddSingleton<IUserValidationService>(userValidationService)
            .AddSingleton<DiscordClientEventHandler>()
            .AddSingleton<DiscordClient>(provider => DiscordClientFactory.Create(
                provider.GetRequiredService<BotSettings>(),
                provider.GetRequiredService<DiscordClientEventHandler>()))
            .AddSingleton(_lavaLinkServiceMock.Object)
            .AddSingleton(_musicQueueServiceMock.Object)
            .AddSingleton(_localizationServiceMock.Object)
            .AddSingleton(_commandHandlerService)
            .BuildServiceProvider();
        
        _discordClient = _serviceProvider.GetRequiredService<DiscordClient>();
    }

    public async Task InitializeAsync()
    {
        await _discordClient.ConnectAsync();
        await Task.Delay(3000);
    }

    public async Task DisposeAsync()
    {
        _commandHandlerService.UnregisterHandler(_discordClient);
        await _discordClient.DisconnectAsync();
        await _serviceProvider.DisposeAsync();
        _discordClient.Dispose();
    }

    private (Mock<ILogger<CommandHandlerService>> freshLoggerMock, CommandHandlerService freshCommandHandlerService, BotSettings botSettings) Init()
    {
        var freshLoggerMock = new Mock<ILogger<CommandHandlerService>>();
        
        freshLoggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName ?? "";

        var envPath = Path.Combine(directoryInfo, ".env");
        Env.Load(envPath);
        
        var envToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        var botSettings = new BotSettings { Token = string.IsNullOrWhiteSpace(envToken) ? "fake-test-token" : envToken, Prefix = BotPrefix };
        
        var userValidationService = new ValidationService(_validationLoggerMock.Object, true);
        
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(botSettings)
            .AddSingleton(_localizationServiceMock.Object)
            .AddSingleton<IUserValidationService>(userValidationService)
            .AddSingleton<IResponseBuilder, ResponseBuilder>()
            .AddSingleton<ICommand, PingCommand>()
            .BuildServiceProvider();

        var freshCommandHandlerService = new CommandHandlerService(
            services, 
            freshLoggerMock.Object,
            _localizationServiceMock.Object, 
            botSettings, 
            true);

        return (freshLoggerMock, freshCommandHandlerService, botSettings);
    }
    
    [Fact]
    public void RegisterHandler_ShouldRegisterEvent()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = Init();
   
        var discordConfig = new DiscordConfiguration
        {
            Token = botSettings.Token ?? "fake-test-token"
        };
        var mockClient = new DiscordClient(discordConfig);

        // Act
        freshCommandHandlerService.RegisterHandler(mockClient);

        // Assert
        freshLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1102),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registered command handler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        freshCommandHandlerService.UnregisterHandler(mockClient);
        mockClient.Dispose();
    }

    [Fact]
    public void UnregisterHandler_ShouldUnregisterEvent()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = Init();
   
        var discordConfig = new DiscordConfiguration
        {
            Token = botSettings.Token ?? "fake-test-token"
        };
        var mockClient = new DiscordClient(discordConfig);

        // Act
        freshCommandHandlerService.RegisterHandler(mockClient);
        freshCommandHandlerService.UnregisterHandler(mockClient);
        
        // Assert
        freshLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1105),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unregistered command handler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        mockClient.Dispose();
    }

    [Fact]
    public async Task HandleCommandAsync_Should_Respond_To_Test_Message()
    {
        _commandHandlerService.RegisterHandler(_discordClient);

        var channel = await _discordClient.GetChannelAsync(TestChannelId);
        var testMessage = await channel.SendMessageAsync("!ping");

        Assert.NotNull(testMessage);

        await Task.Delay(1000);

        var messages = await channel.GetMessagesAsync(1);
        var response = messages.FirstOrDefault();

        Assert.NotNull(response);
        Assert.Contains("Pong", response.Content, StringComparison.OrdinalIgnoreCase);

        _commandHandlerService.UnregisterHandler(_discordClient);
    }

    [Fact]
    public async Task HandleCommandAsync_Responds_To_Unknown_Command()
    {
        _commandHandlerService.RegisterHandler(_discordClient);

        var channel = await _discordClient.GetChannelAsync(TestChannelId);
        var testMessage = await channel.SendMessageAsync("!unknowncommand");

        Assert.NotNull(testMessage);

        await Task.Delay(1000);

        var messages = await channel.GetMessagesAsync(1);
        var response = messages.FirstOrDefault();

        Assert.NotNull(response);
        Assert.Contains("Unknown command.", response.Content, StringComparison.OrdinalIgnoreCase);

        _commandHandlerService.UnregisterHandler(_discordClient);
    }

    [Fact]
    public async Task HandleCommandAsync_Should_Log_No_Prefix_Provided()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = Init();
        freshCommandHandlerService.Prefix = null;
   
        var discordConfig = new DiscordConfiguration
        {
            Token = botSettings.Token ?? "fake-test-token",
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
        };
        var mockClient = new DiscordClient(discordConfig);
        await mockClient.ConnectAsync();
        // Act
        freshCommandHandlerService.RegisterHandler(mockClient);
     
        var channel = await mockClient.GetChannelAsync(TestChannelId);
        await channel.SendMessageAsync("!noPrefix");
        
        await Task.Delay(3000);

        // Assert
        freshLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.Is<EventId>(e => e.Id == 1103),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No prefix provided")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        //cleanup
        freshCommandHandlerService.UnregisterHandler(mockClient);
    }

    [Fact]
    public void UnregisterCommandHandler_Should_Log_Warning()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = Init();
   
        var discordConfig = new DiscordConfiguration
        {
            Token = botSettings.Token ?? "fake-test-token"
        };
        var mockClient = new DiscordClient(discordConfig);

        // Act
        freshCommandHandlerService.UnregisterHandler(mockClient);

        // Assert
        freshLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.Is<EventId>(e => e.Id == 1106),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tried to unregister handler, but it was not registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        mockClient.Dispose();
    }

    [Fact]
    public void RegisterCommandAsync_Twice_Should_Log_Already_Registered()
    {
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = Init();
   
        var discordConfig = new DiscordConfiguration
        {
            Token = botSettings.Token ?? "fake-test-token"
        };
        var mockClient = new DiscordClient(discordConfig);

        // Act
        freshCommandHandlerService.RegisterHandler(mockClient);
        freshCommandHandlerService.RegisterHandler(mockClient);

        // Assert
        freshLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1101),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CommandHandler Service is already registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        freshCommandHandlerService.UnregisterHandler(mockClient);
        mockClient.Dispose();
    }
}