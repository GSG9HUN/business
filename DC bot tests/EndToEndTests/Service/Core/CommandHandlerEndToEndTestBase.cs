using DC_bot.Commands.TextCommands.Utility;
using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using DC_bot.Service.Presentation;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.EndToEndTests.Service.Core;

public abstract class CommandHandlerEndToEndTestBase : IAsyncLifetime
{
    protected const string BotPrefix = "!";

    protected CommandHandlerEndToEndTestBase()
    {
        var hasToken = EndToEndTestConfiguration.TryGetDiscordToken(out var envToken);
        var hasChannel = EndToEndTestConfiguration.TryGetDiscordChannelId(out var testChannelId);
        TestChannelId = testChannelId;
        IsConfigured = hasToken && hasChannel;

        var botSettings = new BotSettings
        { Token = hasToken ? envToken : "fake-test-token", Prefix = BotPrefix };

        SetupLocalization();
        var userValidationService = new ValidationService(ValidationLoggerMock.Object, true);
        var guildDataRepositoryMock = new Mock<IGuildDataRepository>();

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(LocalizationServiceMock.Object)
            .AddSingleton<Func<IEnumerable<ICommand>>>(provider => () => provider.GetServices<ICommand>())
            .AddSingleton<ICommandRegistry, CommandRegistry>()
            .AddSingleton<ICommand, PingCommand>()
            .AddSingleton<ICommand, HelpCommand>()
            .AddSingleton<IResponseBuilder, ResponseBuilder>()
            .AddSingleton<IUserValidationService>(userValidationService)
            .BuildServiceProvider();

        CommandHandlerService = new CommandHandlerService(
            services.GetRequiredService<ICommandRegistry>(),
            CommandServiceLoggerMock.Object,
            LocalizationServiceMock.Object,
            botSettings,
            true);

        ServiceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(botSettings)
            .AddSingleton<IUserValidationService>(userValidationService)
            .AddSingleton(guildDataRepositoryMock.Object)
            .AddSingleton<DiscordClientEventHandler>()
            .AddSingleton<DiscordClient>(provider => DiscordClientFactory.Create(
                provider.GetRequiredService<BotSettings>()))
            .AddSingleton(LavaLinkServiceMock.Object)
            .AddSingleton(MusicQueueServiceMock.Object)
            .AddSingleton(LocalizationServiceMock.Object)
            .AddSingleton(CommandHandlerService)
            .BuildServiceProvider();

        DiscordClient = ServiceProvider.GetRequiredService<DiscordClient>();
    }

    protected CommandHandlerService CommandHandlerService { get; }
    protected Mock<ILogger<CommandHandlerService>> CommandServiceLoggerMock { get; } = new();
    protected DiscordClient DiscordClient { get; }
    protected bool IsConfigured { get; }
    protected Mock<ILavaLinkService> LavaLinkServiceMock { get; } = new();
    protected Mock<ILocalizationService> LocalizationServiceMock { get; } = new();
    protected Mock<IMusicQueueService> MusicQueueServiceMock { get; } = new();
    protected ServiceProvider ServiceProvider { get; }
    protected ulong TestChannelId { get; }
    protected bool IsDiscordAvailable { get; private set; }
    protected Mock<ILogger<ValidationService>> ValidationLoggerMock { get; } = new();

    public async Task InitializeAsync()
    {
        if (!IsConfigured) return;
        IsDiscordAvailable = await EndToEndDiscordGuard.TryConnectAndWaitUntilReadyAsync(DiscordClient);
    }

    public async Task DisposeAsync()
    {
        if (IsConfigured)
        {
            CommandHandlerService.UnregisterHandler(DiscordClient);
            await EndToEndDiscordGuard.DisconnectIgnoringDisconnectedGatewayAsync(DiscordClient);
        }

        await ServiceProviderDisposeHelper.DisposeIgnoringDisconnectedDiscordClientAsync(ServiceProvider);
        DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(DiscordClient);
    }

    protected (Mock<ILogger<CommandHandlerService>> freshLoggerMock, CommandHandlerService freshCommandHandlerService,
        BotSettings botSettings) CreateFreshCommandHandler()
    {
        var freshLoggerMock = new Mock<ILogger<CommandHandlerService>>();
        freshLoggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var hasToken = EndToEndTestConfiguration.TryGetDiscordToken(out var envToken);
        var botSettings = new BotSettings
        { Token = hasToken ? envToken : "fake-test-token", Prefix = BotPrefix };

        var userValidationService = new ValidationService(ValidationLoggerMock.Object, true);

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(botSettings)
            .AddSingleton(LocalizationServiceMock.Object)
            .AddSingleton<IUserValidationService>(userValidationService)
            .AddSingleton<IResponseBuilder, ResponseBuilder>()
            .AddSingleton<Func<IEnumerable<ICommand>>>(provider => () => provider.GetServices<ICommand>())
            .AddSingleton<ICommandRegistry, CommandRegistry>()
            .AddSingleton<ICommand, PingCommand>()
            .AddSingleton<ICommand, HelpCommand>()
            .BuildServiceProvider();

        var freshCommandHandlerService = new CommandHandlerService(
            services.GetRequiredService<ICommandRegistry>(),
            freshLoggerMock.Object,
            LocalizationServiceMock.Object,
            botSettings,
            true);

        return (freshLoggerMock, freshCommandHandlerService, botSettings);
    }

    protected async Task<DiscordChannel?> GetTestChannelAsync(DiscordClient client)
    {
        try
        {
            return await client.GetChannelAsync(TestChannelId);
        }
        catch (Exception exception) when (EndToEndDiscordGuard.IsDiscordEnvironmentUnavailable(exception))
        {
            return null;
        }
    }

    protected bool CanRun()
    {
        return IsConfigured && IsDiscordAvailable;
    }

    private void SetupLocalization()
    {
        LocalizationServiceMock.Setup(ls => ls.Get(LocalizationKeys.UnknownCommandError))
            .Returns("Unknown command. Use `!help` to see available commands.");
        LocalizationServiceMock.Setup(ls => ls.Get(It.IsAny<ulong>(), LocalizationKeys.UnknownCommandError))
            .Returns("Unknown command. Use `!help` to see available commands.");
        LocalizationServiceMock.Setup(ls => ls.Get(LocalizationKeys.PingCommandDescription))
            .Returns("Replies with Pong.");
        LocalizationServiceMock.Setup(ls => ls.Get(It.IsAny<ulong>(), LocalizationKeys.PingCommandDescription))
            .Returns("Replies with Pong.");
        LocalizationServiceMock.Setup(ls => ls.Get(LocalizationKeys.HelpCommandDescription))
            .Returns("Lists the available commands.");
        LocalizationServiceMock.Setup(ls => ls.Get(It.IsAny<ulong>(), LocalizationKeys.HelpCommandDescription))
            .Returns("Lists the available commands.");
        LocalizationServiceMock.Setup(ls => ls.Get(LocalizationKeys.HelpCommandResponse))
            .Returns("Available commands:");
        LocalizationServiceMock.Setup(ls => ls.Get(It.IsAny<ulong>(), LocalizationKeys.HelpCommandResponse))
            .Returns("Available commands:");
        LocalizationServiceMock
            .Setup(ls => ls.Get(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<string, object[]>(FormatLocalization);
        LocalizationServiceMock
            .Setup(ls => ls.Get(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<ulong, string, object[]>((_, key, args) => FormatLocalization(key, args));
        LocalizationServiceMock
            .Setup(ls => ls.Get(
                It.IsAny<ulong>(),
                LocalizationKeys.HelpCommandResponse,
                It.IsAny<object[]>()))
            .Returns<ulong, string, object[]>((_, key, args) => FormatLocalization(key, args));
    }

    private static string FormatLocalization(string key, object[] args)
    {
        return key switch
        {
            LocalizationKeys.UnknownCommandError => "Unknown command. Use `!help` to see available commands.",
            LocalizationKeys.PingCommandDescription => "Replies with Pong.",
            LocalizationKeys.PingCommandResponse => "Pong!",
            LocalizationKeys.HelpCommandDescription => "Lists the available commands.",
            LocalizationKeys.HelpCommandResponse => args.Length > 0
                ? $"Available commands:{Environment.NewLine}{args[0]}"
                : "Available commands:",
            _ => args.Length == 0 ? key : string.Format(key, args)
        };
    }
}
