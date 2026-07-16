using System.Reflection;
using DC_bot.Commands.TextCommands.Music;
using DC_bot.Commands.TextCommands.Queue;
using DC_bot.Commands.TextCommands.Utility;
using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service.Core;

internal static class CommandHandlerIntegrationFixture
{
    public static async Task InvokeHandleCommandAsync(
        CommandHandlerService service,
        MessageCreatedEventArgs args)
    {
        var method = typeof(CommandHandlerService).GetMethod("HandleCommandAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(method);

        var client = DiscordClientBuilder
            .CreateDefault("fake-token", DiscordIntents.AllUnprivileged)
            .Build();

        var task = (Task?)method.Invoke(service, [client, args]);
        Assert.NotNull(task);
        await task;
    }

    public static CommandHandlerService CreateCommandHandler(
        IServiceProvider services,
        ILocalizationService localizationService,
        IDiscordMessageFactory? messageFactory = null)
    {
        var logger = new Mock<ILogger<CommandHandlerService>>();
        logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        return new CommandHandlerService(
            services.GetRequiredService<ICommandRegistry>(),
            logger.Object,
            localizationService,
            new BotSettings { Prefix = "!" },
            isTestMode: true,
            messageFactory);
    }

    public static CommandHandlerService CreateHandlerWithCommand(
        ICommand command,
        bool isTestMode = true)
    {
        var commandRegistry = new CommandRegistry(() => [command]);
        var logger = new Mock<ILogger<CommandHandlerService>>();
        logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        return new CommandHandlerService(
            commandRegistry,
            logger.Object,
            Mock.Of<ILocalizationService>(),
            new BotSettings { Prefix = "!" },
            isTestMode: isTestMode);
    }

    public static RealTextCommandGraph CreateRealTextCommandGraph()
    {
        var responseBuilderMock = new Mock<IResponseBuilder>();
        var localizationServiceMock = CreateLocalizationServiceMock();
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var musicQueueServiceMock = new Mock<IMusicQueueService>();
        var messageFactory = new CommandHandlerFakeMessageFactory();

        var validationService = new ValidationService(Mock.Of<ILogger<ValidationService>>());

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(responseBuilderMock.Object)
            .AddSingleton(localizationServiceMock.Object)
            .AddSingleton<IValidationService>(validationService)
            .AddSingleton<IUserValidationService>(validationService)
            .AddSingleton<ICommandHelper, CommandValidationService>()
            .AddSingleton<Func<IEnumerable<ICommand>>>(provider => () => provider.GetServices<ICommand>())
            .AddSingleton<ICommandRegistry, CommandRegistry>()
            .AddSingleton(lavaLinkServiceMock.Object)
            .AddSingleton(musicQueueServiceMock.Object)
            .AddSingleton(Mock.Of<IRepeatService>())
            .AddSingleton(Mock.Of<ICurrentTrackService>())
            .AddSingleton(Mock.Of<ITrackFormatterService>())
            .AddSingleton(Mock.Of<ITrackSearchResolverService>())
            .AddSingleton<ICommand, TagCommand>()
            .AddSingleton<ICommand, JoinCommand>()
            .AddSingleton<ICommand, PingCommand>()
            .AddSingleton<ICommand, HelpCommand>()
            .AddSingleton<ICommand, PlayCommand>()
            .AddSingleton<ICommand, SkipCommand>()
            .AddSingleton<ICommand, ClearCommand>()
            .AddSingleton<ICommand, LeaveCommand>()
            .AddSingleton<ICommand, PauseCommand>()
            .AddSingleton<ICommand, ResumeCommand>()
            .AddSingleton<ICommand, RepeatCommand>()
            .AddSingleton<ICommand, ShuffleCommand>()
            .AddSingleton<ICommand, LanguageCommand>()
            .AddSingleton<ICommand, ViewQueueCommand>()
            .AddSingleton<ICommand, RepeatListCommand>()
            .BuildServiceProvider();

        _ = services.GetServices<ICommand>().ToArray();

        return new RealTextCommandGraph(
            services,
            responseBuilderMock,
            localizationServiceMock,
            lavaLinkServiceMock,
            musicQueueServiceMock,
            messageFactory);
    }

    private static Mock<ILocalizationService> CreateLocalizationServiceMock()
    {
        var localizationService = new Mock<ILocalizationService>();

        localizationService
            .Setup(service => service.Get(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<string, object[]>(FormatLocalization);
        localizationService
            .Setup(service => service.Get(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<ulong, string, object[]>((_, key, args) => FormatLocalization(key, args));

        return localizationService;
    }

    private static string FormatLocalization(string key, object[] args)
    {
        return key switch
        {
            LocalizationKeys.HelpCommandResponse => "Available commands:",
            LocalizationKeys.LanguageCommandResponse => "The language changed successfully.",
            _ => args.Length == 0 ? key : string.Format(key, args)
        };
    }

    internal sealed record RealTextCommandGraph(
        ServiceProvider Services,
        Mock<IResponseBuilder> ResponseBuilderMock,
        Mock<ILocalizationService> LocalizationServiceMock,
        Mock<ILavaLinkService> LavaLinkServiceMock,
        Mock<IMusicQueueService> MusicQueueServiceMock,
        IDiscordMessageFactory MessageFactory) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            return Services.DisposeAsync();
        }
    }
}
