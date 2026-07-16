using DC_bot.Commands.TextCommands.Utility;
using DC_bot.Configuration;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using DC_bot.Service.Presentation;
using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.EndToEndTests.Service.Core;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class CommandHandlerGuardEndToEndTests : CommandHandlerEndToEndTestBase
{
    [Fact]
    public async Task HandleCommandAsync_Should_Log_No_Prefix_Provided()
    {
        if (!CanRun()) return;
        var (freshLoggerMock, freshCommandHandlerService, botSettings) = CreateFreshCommandHandler();
        freshCommandHandlerService.Prefix = null;

        var mockClient = TestDiscordClientFactory.Create(
            botSettings.Token ?? "fake-test-token",
            DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents);
        if (!await EndToEndDiscordGuard.TryConnectAndWaitUntilReadyAsync(mockClient))
        {
            DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(mockClient);
            return;
        }

        try
        {
            freshCommandHandlerService.RegisterHandler(mockClient);
            freshLoggerMock.Invocations.Clear();
            var channel = await GetTestChannelAsync(mockClient);
            if (channel is null) return;
            var guild = DiscordEventArgsFactory.CreateGuild(channel.GuildId!.Value);
            var message = await channel.SendMessageAsync("!noPrefix");

            await freshCommandHandlerService.HandleEventAsync(mockClient,
                DiscordEventArgsFactory.CreateMessageCreated(message, guild));

            freshLoggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.Is<EventId>(e => e.Id == 1103),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("No prefix provided")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.AtLeastOnce);
        }
        finally
        {
            freshCommandHandlerService.UnregisterHandler(mockClient);
            await EndToEndDiscordGuard.DisconnectIgnoringDisconnectedGatewayAsync(mockClient);
            DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(mockClient);
        }
    }

    [Fact]
    public async Task HandleCommandAsync_WhenAuthorIsBot_AndIsTestModeFalse_IgnoresCommand()
    {
        if (!CanRun()) return;
        EndToEndTestConfiguration.TryGetDiscordToken(out var envToken);

        var botSettings = new BotSettings { Token = envToken, Prefix = BotPrefix };
        var loggerMock = new Mock<ILogger<CommandHandlerService>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var userValidationService = new ValidationService(ValidationLoggerMock.Object);
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(botSettings)
            .AddSingleton(LocalizationServiceMock.Object)
            .AddSingleton<IUserValidationService>(userValidationService)
            .AddSingleton<IResponseBuilder, ResponseBuilder>()
            .AddSingleton<Func<IEnumerable<ICommand>>>(provider => () => provider.GetServices<ICommand>())
            .AddSingleton<ICommandRegistry, CommandRegistry>()
            .AddSingleton<ICommand, PingCommand>()
            .BuildServiceProvider();

        var nonTestHandler = new CommandHandlerService(
            services.GetRequiredService<ICommandRegistry>(),
            loggerMock.Object,
            LocalizationServiceMock.Object,
            botSettings);

        nonTestHandler.RegisterHandler(DiscordClient);

        var channel = await GetTestChannelAsync(DiscordClient);
        if (channel is null) return;
        var guild = DiscordEventArgsFactory.CreateGuild(channel.GuildId!.Value);
        var marker = $"e2e-ignore-bot-{Guid.NewGuid():N}";
        var markerMessage = await channel.SendMessageAsync($"!ping {marker}");

        await nonTestHandler.HandleEventAsync(DiscordClient,
            DiscordEventArgsFactory.CreateMessageCreated(markerMessage, guild));
        await DiscordMessageWaiter.AssertNoMessageAfterAsync(
            channel,
            markerMessage.Id,
            message => message.Content.Contains("Pong", StringComparison.OrdinalIgnoreCase),
            "Pong response for bot-authored command",
            quietPeriod: TimeSpan.FromMilliseconds(1200));

        nonTestHandler.UnregisterHandler(DiscordClient);
    }
}
