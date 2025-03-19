using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Service;
using DC_bot.Wrapper;
using DotNetEnv;
using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.IntegrationTests.Wrapper;

[Collection("Integration Tests")]
public class CommandHandlerServiceTest
{
    private readonly Mock<ILogger<SingletonDiscordClient>> _loggerSingletonDiscordClientMock = new();
    private readonly Mock<ILogger<CommandHandlerService>> _commandServiceLoggerMock = new();
    private readonly Mock<ILogger<UserValidationService>> _userValidationLoggerMock = new();
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock = new();
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock = new();
    private const ulong TestChannelId = 1339151008307351572;
    private readonly DiscordClient _discordClient;
    private readonly CommandHandlerService _commandHandlerService;

    public CommandHandlerServiceTest()
    {
        var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;

        var envPath = Path.Combine(directoryInfo, ".env");
        Env.Load(envPath);

        _localizationServiceMock.Setup(ls => ls.Get("unknown_command_error"))
            .Returns("Unknown command. Use `!help` to see available commands.");
        var userValidationService =
            new UserValidationService(_userValidationLoggerMock.Object, _localizationServiceMock.Object, true);

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(_localizationServiceMock.Object)
            .AddSingleton<ICommand, PingCommand>()
            .AddSingleton<IUserValidationService>(userValidationService)
            .BuildServiceProvider();

        _commandHandlerService = new CommandHandlerService(services, _commandServiceLoggerMock.Object,
            _localizationServiceMock.Object, true);

        var service = new ServiceCollection()
            .AddSingleton<IUserValidationService>(userValidationService)
            .AddSingleton(_lavaLinkServiceMock.Object)
            .AddSingleton(_musicQueueServiceMock.Object)
            .AddSingleton(_localizationServiceMock.Object)
            .AddSingleton(_commandHandlerService).BuildServiceProvider();

        ServiceLocator.SetServiceProvider(service);

        SingletonDiscordClient.InitializeLogger(_loggerSingletonDiscordClientMock.Object);
        _discordClient = SingletonDiscordClient.Instance;
    }

    [Fact]
    public void RegisterHandler_ShouldRegisterEvent()
    {
        _commandHandlerService.RegisterHandler(_discordClient);

        _commandServiceLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registered command handler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        _commandHandlerService.UnRegisterHandler(_discordClient);
    }

    [Fact]
    public async Task HandleCommandAsync_Should_Respond_To_Test_Message()
    {
        _commandHandlerService.RegisterHandler(_discordClient);
        await _discordClient.ConnectAsync();
        await Task.Delay(10000);

        var channel = await _discordClient.GetChannelAsync(TestChannelId);

        var testMessage = await channel.SendMessageAsync("!ping");

        Assert.NotNull(testMessage);

        await Task.Delay(2000);

        var message = await channel.GetMessagesAsync(1);
        var response = message.FirstOrDefault();

        Assert.NotNull(response);
        Assert.Contains("Pong", response.Content, StringComparison.OrdinalIgnoreCase);

        _commandHandlerService.UnRegisterHandler(_discordClient);

        await _discordClient.DisconnectAsync();
        await Task.Delay(10000);
    }

    [Fact]
    public async Task HandleCommandAsync_Responds_To_Unknown_Command()
    {
        _commandHandlerService.RegisterHandler(_discordClient);
        await _discordClient.ConnectAsync();
        await Task.Delay(10000);

        var channel = await _discordClient.GetChannelAsync(TestChannelId);

        var testMessage = await channel.SendMessageAsync("!unknowncommand");

        Assert.NotNull(testMessage);

        await Task.Delay(2000);

        var message = await channel.GetMessagesAsync(1);
        var response = message.FirstOrDefault();

        Assert.NotNull(response);
        Assert.Contains("Unknown command.", response.Content, StringComparison.OrdinalIgnoreCase);
        _commandHandlerService.UnRegisterHandler(_discordClient);
        await _discordClient.DisconnectAsync();
        await Task.Delay(10000);
    }

    [Fact]
    public async Task HanldeCommandAsync_Should_Log_No_Prefix_Provided()
    {
        _commandHandlerService.prefix = null;
        _commandHandlerService.RegisterHandler(_discordClient);

        await _discordClient.ConnectAsync();
        await Task.Delay(10000);

        var channel = await _discordClient.GetChannelAsync(TestChannelId);

        var testMessage = await channel.SendMessageAsync("!noPrefix");

        Assert.NotNull(testMessage);

        await Task.Delay(2000);
        _commandServiceLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No prefix provided")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
        _commandHandlerService.UnRegisterHandler(_discordClient);

        await _discordClient.DisconnectAsync();
        await Task.Delay(10000);
    }

    [Fact]
    public void UnregisterCommandHandler_Should_Log_Warning()
    {
        _commandHandlerService.UnRegisterHandler(_discordClient);

        _commandServiceLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Tried to unregister handler, but it was not registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void RegisterCommandAsync_Twice_Should_Log_Already_Regsitered()
    {
        _commandHandlerService.RegisterHandler(_discordClient);
        _commandHandlerService.RegisterHandler(_discordClient);

        _commandServiceLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("CommandHandler Service is already registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
        
        _commandHandlerService.UnRegisterHandler(_discordClient);
    }
}