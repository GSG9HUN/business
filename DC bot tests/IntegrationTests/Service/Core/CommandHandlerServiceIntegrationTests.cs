using System.Reflection;
using System.Runtime.CompilerServices;
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
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service.Core;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class CommandHandlerServiceIntegrationTests
{
    [Fact]
    public async Task HandleCommandAsync_WithFakeDiscordMessageEvent_ExecutesRegisteredCommand()
    {
        var command = new Mock<ICommand>();
        var executed = new TaskCompletionSource<IDiscordMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        command.SetupGet(x => x.Name).Returns("fake");
        command.SetupGet(x => x.Description).Returns("Fake command");
        command.Setup(x => x.ExecuteAsync(It.IsAny<IDiscordMessage>()))
            .Callback<IDiscordMessage>(executed.SetResult)
            .Returns(Task.CompletedTask);

        var commandRegistry = new CommandRegistry(() => [command.Object]);

        var logger = new Mock<ILogger<CommandHandlerService>>();
        logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var service = new CommandHandlerService(
            commandRegistry,
            logger.Object,
            Mock.Of<ILocalizationService>(),
            new BotSettings { Prefix = "!" },
            isTestMode: true);
        var args = CreateMessageCreateEventArgs("!fake with args", isBot: false);

        await InvokeHandleCommandAsync(service, args);

        var handledMessage = await executed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal("!fake with args", handledMessage.Content);
        command.Verify(x => x.ExecuteAsync(It.IsAny<IDiscordMessage>()), Times.Once);
    }

    [Fact]
    public async Task HandleCommandAsync_WithFakeDiscordMessageWithoutPrefix_DoesNotExecuteCommand()
    {
        var command = new Mock<ICommand>();
        command.SetupGet(x => x.Name).Returns("fake");
        command.SetupGet(x => x.Description).Returns("Fake command");

        var commandRegistry = new CommandRegistry(() => [command.Object]);

        var service = new CommandHandlerService(
            commandRegistry,
            Mock.Of<ILogger<CommandHandlerService>>(),
            Mock.Of<ILocalizationService>(),
            new BotSettings { Prefix = "!" },
            isTestMode: true);
        var args = CreateMessageCreateEventArgs("fake without prefix", isBot: false);

        await InvokeHandleCommandAsync(service, args);

        command.Verify(x => x.ExecuteAsync(It.IsAny<IDiscordMessage>()), Times.Never);
    }

    [Fact]
    public async Task HandleCommandAsync_WithBotAuthorAndProductionMode_DoesNotExecuteCommand()
    {
        var command = new Mock<ICommand>();
        command.SetupGet(x => x.Name).Returns("fake");
        command.SetupGet(x => x.Description).Returns("Fake command");

        var commandRegistry = new CommandRegistry(() => [command.Object]);

        var service = new CommandHandlerService(
            commandRegistry,
            Mock.Of<ILogger<CommandHandlerService>>(),
            Mock.Of<ILocalizationService>(),
            new BotSettings { Prefix = "!" });
        var args = CreateMessageCreateEventArgs("!fake", isBot: true);

        await InvokeHandleCommandAsync(service, args);

        command.Verify(x => x.ExecuteAsync(It.IsAny<IDiscordMessage>()), Times.Never);
    }

    [Fact]
    public async Task HandleCommandAsync_WithRealCommandList_RoutesUtilityCommandsThroughFakeDiscordMessages()
    {
        await using var graph = CreateRealTextCommandGraph();
        var service = CreateCommandHandler(
            graph.Services,
            graph.LocalizationServiceMock.Object,
            graph.MessageFactory);

        await InvokeHandleCommandAsync(service, CreateMessageCreateEventArgs("!ping", isBot: false));
        await InvokeHandleCommandAsync(service, CreateMessageCreateEventArgs("!help", isBot: false));
        await InvokeHandleCommandAsync(service, CreateMessageCreateEventArgs("!language hu", isBot: false));
        await InvokeHandleCommandAsync(service, CreateMessageCreateEventArgs("!tag", isBot: false));

        graph.ResponseBuilderMock.Verify(
            response => response.SendSuccessAsync(
                It.IsAny<IDiscordMessage>(),
                LocalizationKeys.PingCommandResponse),
            Times.Once);
        graph.ResponseBuilderMock.Verify(
            response => response.SendSuccessAsync(
                It.IsAny<IDiscordMessage>(),
                LocalizationKeys.HelpCommandResponse,
                It.Is<object[]>(args =>
                    args.Length == 1 &&
                    args[0].ToString()!.Contains("ping", StringComparison.Ordinal) &&
                    args[0].ToString()!.Contains("play", StringComparison.Ordinal))),
            Times.Once);
        graph.LocalizationServiceMock.Verify(
            localization => localization.SaveLanguage(456UL, "hu"),
            Times.Once);
        graph.ResponseBuilderMock.Verify(
            response => response.SendSuccessAsync(
                It.IsAny<IDiscordMessage>(),
                LocalizationKeys.LanguageCommandResponse),
            Times.Once);
        graph.ResponseBuilderMock.Verify(
            response => response.SendUsageAsync(It.IsAny<IDiscordMessage>(), "tag"),
            Times.Once);
    }

    [Fact]
    public async Task HandleCommandAsync_WithRealCommandList_RoutesMusicAndQueueCommandsThroughValidationGuards()
    {
        await using var graph = CreateRealTextCommandGraph();
        var service = CreateCommandHandler(
            graph.Services,
            graph.LocalizationServiceMock.Object,
            graph.MessageFactory);

        await InvokeHandleCommandAsync(service, CreateMessageCreateEventArgs("!join", isBot: false));
        await InvokeHandleCommandAsync(service, CreateMessageCreateEventArgs("!clear", isBot: false));

        graph.ResponseBuilderMock.Verify(
            response => response.SendValidationErrorAsync(
                It.IsAny<IDiscordMessage>(),
                ValidationErrorKeys.UserNotInVoiceChannel),
            Times.Exactly(2));
        graph.LavaLinkServiceMock.Verify(
            lavaLinkService => lavaLinkService.StartPlayingQueue(
                It.IsAny<IDiscordMessage>(),
                It.IsAny<IDiscordChannel>(),
                It.IsAny<IDiscordMember>()),
            Times.Never);
        graph.MusicQueueServiceMock.Verify(
            musicQueueService => musicQueueService.ClearQueue(It.IsAny<ulong>()),
            Times.Never);
    }

    private static async Task InvokeHandleCommandAsync(CommandHandlerService service, MessageCreatedEventArgs args)
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

    private static MessageCreatedEventArgs CreateMessageCreateEventArgs(string content, bool isBot)
    {
        var author = Create<DiscordUser>();
        SetMember(author, "Id", 123ul);
        SetMember(author, "Username", "IntegrationUser");
        SetMember(author, "IsBot", isBot);

        var guild = Create<DiscordGuild>();
        SetMember(guild, "Id", 456ul);
        SetMember(guild, "Name", "IntegrationGuild");

        var channel = Create<DiscordChannel>();
        SetMember(channel, "Id", 789ul);
        SetMember(channel, "Name", "integration-channel");
        TrySetMember(channel, "Guild", guild);

        var message = Create<DiscordMessage>();
        SetMember(message, "Id", 999ul);
        SetMember(message, "Content", content);
        SetMember(message, "embeds", new List<DiscordEmbed>());
        TrySetMember(message, "Author", author);
        TrySetMember(message, "Channel", channel);

        var args = Create<MessageCreatedEventArgs>();
        SetMember(args, "Message", message);
        TrySetMember(args, "Author", author);
        TrySetMember(args, "Channel", channel);
        TrySetMember(args, "Guild", guild);

        return args;
    }

    private static CommandHandlerService CreateCommandHandler(
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

    private static RealTextCommandGraph CreateRealTextCommandGraph()
    {
        var responseBuilderMock = new Mock<IResponseBuilder>();
        var localizationServiceMock = CreateLocalizationServiceMock();
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var musicQueueServiceMock = new Mock<IMusicQueueService>();
        var messageFactory = new TestDiscordMessageFactory();

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

    private sealed record RealTextCommandGraph(
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

    private sealed class TestDiscordMessageFactory : IDiscordMessageFactory
    {
        public IDiscordMessage Create(
            DiscordMessage message,
            DiscordChannel channel,
            DiscordUser author,
            DiscordGuild? guild = null)
        {
            var voiceState = new Mock<IDiscordVoiceState>();
            voiceState.SetupGet(x => x.Channel).Returns((IDiscordChannel?)null);

            var member = new Mock<IDiscordMember>();
            member.SetupGet(x => x.Id).Returns(author.Id);
            member.SetupGet(x => x.Username).Returns("IntegrationUser");
            member.SetupGet(x => x.Mention).Returns("<@123>");
            member.SetupGet(x => x.IsBot).Returns(author.IsBot);
            member.SetupGet(x => x.VoiceState).Returns(voiceState.Object);

            var discordGuild = new Mock<IDiscordGuild>();
            discordGuild.SetupGet(x => x.Id).Returns(456UL);
            discordGuild.SetupGet(x => x.Name).Returns("IntegrationGuild");
            discordGuild.Setup(x => x.GetMemberAsync(author.Id)).ReturnsAsync(member.Object);
            discordGuild.Setup(x => x.GetAllMembersAsync()).ReturnsAsync([member.Object]);

            var discordChannel = new Mock<IDiscordChannel>();
            discordChannel.SetupGet(x => x.Id).Returns(channel.Id);
            discordChannel.SetupGet(x => x.Name).Returns("integration-channel");
            discordChannel.SetupGet(x => x.Guild).Returns(discordGuild.Object);

            var discordUser = new Mock<IDiscordUser>();
            discordUser.SetupGet(x => x.Id).Returns(author.Id);
            discordUser.SetupGet(x => x.Username).Returns("IntegrationUser");
            discordUser.SetupGet(x => x.Mention).Returns("<@123>");
            discordUser.SetupGet(x => x.IsBot).Returns(author.IsBot);

            var discordMessage = new Mock<IDiscordMessage>();
            discordMessage.SetupGet(x => x.Id).Returns(message.Id);
            discordMessage.SetupGet(x => x.Content).Returns(message.Content);
            discordMessage.SetupGet(x => x.Author).Returns(discordUser.Object);
            discordMessage.SetupGet(x => x.Channel).Returns(discordChannel.Object);
            discordMessage.SetupGet(x => x.CreatedAt).Returns(message.CreationTimestamp);
            discordMessage.SetupGet(x => x.Embeds).Returns(message.Embeds.ToList());
            return discordMessage.Object;
        }
    }

    private static T Create<T>() => (T)RuntimeHelpers.GetUninitializedObject(typeof(T));

    private static bool TrySetMember(object obj, string name, object? value)
    {
        var type = obj.GetType();
        while (type is not null)
        {
            var backingField = type.GetField($"<{name}>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (backingField is not null)
            {
                backingField.SetValue(obj, value);
                return true;
            }

            var underscored = type.GetField($"_{char.ToLowerInvariant(name[0])}{name[1..]}",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (underscored is not null)
            {
                underscored.SetValue(obj, value);
                return true;
            }

            var matchingField = type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(field =>
                    string.Equals(field.Name, name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(field.Name.TrimStart('_'), name, StringComparison.OrdinalIgnoreCase));
            if (matchingField is not null)
            {
                matchingField.SetValue(obj, value);
                return true;
            }

            var property = type.GetProperty(name,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (property?.SetMethod is not null)
            {
                property.SetValue(obj, value);
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    private static void SetMember(object obj, string name, object? value)
    {
        if (!TrySetMember(obj, name, value))
        {
            throw new InvalidOperationException($"Member '{name}' not found on {obj.GetType().Name}.");
        }
    }
}
