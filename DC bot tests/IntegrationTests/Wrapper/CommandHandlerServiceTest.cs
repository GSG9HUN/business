using System.Diagnostics;
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

public class CommandHandlerServiceTest
{
    private readonly Mock<ILogger<SingletonDiscordClient>> _loggerSingletonDiscordClientMock = new();
    private readonly Mock<ILogger<CommandHandlerService>> _commandServiceLoggerMock = new();
    private readonly Mock<ILogger<UserValidationService>> _userValidationLoggerMock = new();
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock = new();
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock = new();
    private readonly ulong _testChannelId = 1339151008307351572;
    private readonly DiscordClient _discordClient;
    private readonly CommandHandlerService _commandHandlerService;
    
    public CommandHandlerServiceTest()
    {
        var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;

        var envPath = Path.Combine(directoryInfo, ".env");
        Env.Load(envPath);

        // Mock localization service
        _localizationServiceMock.Setup(ls => ls.Get("unknown_command_error"))
            .Returns("Unknown command. Use `!help` to see available commands.");
        
        var userValidation =
            new UserValidationService(_userValidationLoggerMock.Object, _localizationServiceMock.Object, true);
        var services = new ServiceCollection() 
            .AddLogging(builder => { builder.AddConsole().SetMinimumLevel(LogLevel.Debug); })
            .AddSingleton<IUserValidationService>(userValidation)
            .AddSingleton(_lavaLinkServiceMock.Object)
            .AddSingleton(_musicQueueServiceMock.Object)
            .AddSingleton(_localizationServiceMock.Object)
            .AddSingleton<ICommand, PingCommand>().BuildServiceProvider();
        
        _commandHandlerService = new CommandHandlerService(services, _commandServiceLoggerMock.Object,
            _localizationServiceMock.Object, true)
        {
            prefix = null
        };

        var service = new ServiceCollection()
            .AddSingleton<IUserValidationService>(userValidation)
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
    }
/*
    [Fact]
    public async Task Bot_Should_Respond_To_Test_Message()
    {
        _discordClient.DisconnectAsync().GetAwaiter();
        await _discordClient.ConnectAsync();
        await Task.Delay(5000);

        var channel = await _discordClient.GetChannelAsync(_testChannelId);

        var testMessage = await channel.SendMessageAsync("!ping");

        Assert.NotNull(testMessage);

        await Task.Delay(5000);

        var message = await channel.GetMessagesAsync(1);
        var response = message.FirstOrDefault();

        Assert.NotNull(response);
        Assert.Contains("Pong", response.Content, StringComparison.OrdinalIgnoreCase);
        
        _discordClient.DisconnectAsync().GetAwaiter();
        await Task.Delay(5000);
    }*/
    
    [Fact]
    public async Task HandleCommandAsync_Should_LogError_WhenPrefixIsNull()
    {       
        _discordClient.DisconnectAsync().GetAwaiter();
        await _discordClient.ConnectAsync();
        await Task.Delay(5000);

        var channel = await _discordClient.GetChannelAsync(_testChannelId);

        var testMessage = await channel.SendMessageAsync("!noPrefix");

        Assert.NotNull(testMessage);

        await Task.Delay(5000);
        /*_commandServiceLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registered command handler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );*/

        _commandServiceLoggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback((LogLevel level, EventId id, object state, Exception ex, Delegate formatter) =>
            {
                Debug.WriteLine($"[DEBUG LOG] {level}: {state}");
            });


        _commandServiceLoggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),   // 🔹 LogLevel pontos egyezés
                It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((obj, t) => obj.ToString().Contains("No prefix provided")),  // 🔹 Üzenet keresése
                It.IsAny<Exception>(), 
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), 
            Times.Once // 🔹 Csak egyszer kell meghívódnia
        );

        
        _discordClient.DisconnectAsync().GetAwaiter();
        await Task.Delay(5000);
    }
/*
    [Fact]
    public async Task HandleCommandAsync_Responds_To_Unknown_Command()
    {
        _discordClient.DisconnectAsync().GetAwaiter();
        await _discordClient.ConnectAsync();
        await Task.Delay(5000);

        var channel = await _discordClient.GetChannelAsync(_testChannelId);

        var testMessage = await channel.SendMessageAsync("!unknowncommand");

        Assert.NotNull(testMessage);

        await Task.Delay(5000);

        var message = await channel.GetMessagesAsync(1);
        var response = message.FirstOrDefault();

        Assert.NotNull(response);
        Assert.Contains("Unknown command.", response.Content, StringComparison.OrdinalIgnoreCase);
        
        _discordClient.DisconnectAsync().GetAwaiter();
        
        await Task.Delay(5000);
    }*/
}