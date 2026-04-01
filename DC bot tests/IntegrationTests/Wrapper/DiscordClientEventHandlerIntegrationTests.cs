using DC_bot.Configuration;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Wrapper;
using DotNetEnv;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.IntegrationTests.Wrapper;

public class DiscordClientEventHandlerIntegrationTests
{
    [Fact]
    public async Task OnGuildAvailable_WithNullArgs_LogsError()
    {
        var loggerMock = new Mock<ILogger<DiscordClientEventHandler>>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var guildDataRepositoryMock = new Mock<IGuildDataRepository>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var eventHandler = new DiscordClientEventHandler(loggerMock.Object, guildDataRepositoryMock.Object,
            serviceProviderMock.Object);

        await eventHandler.OnGuildAvailable(null!, null!);

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.Is<EventId>(e => e.Id == 1504),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Discord client event failed") &&
                    v.ToString()!.Contains("OnGuildAvailable")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnGuildAvailable_WithNullArgs_DoesNotResolveServices()
    {
        var loggerMock = new Mock<ILogger<DiscordClientEventHandler>>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var guildDataRepositoryMock = new Mock<IGuildDataRepository>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var eventHandler = new DiscordClientEventHandler(loggerMock.Object, guildDataRepositoryMock.Object,
            serviceProviderMock.Object);

        await eventHandler.OnGuildAvailable(null!, null!);

        serviceProviderMock.Verify(sp => sp.GetService(typeof(ILavaLinkService)), Times.Never);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(ILocalizationService)), Times.Never);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IMusicQueueService)), Times.Never);
    }

    [Fact]
    public async Task OnGuildAvailable_Call_GetRequiredService_Three_Times()
    {
        var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName ??
                            "";

        var envPath = Path.Combine(directoryInfo, ".env");
        Env.Load(envPath);

        var envToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        var botSettings = new BotSettings
            { Token = string.IsNullOrWhiteSpace(envToken) ? "fake-test-token" : envToken, Prefix = "!" };

        var discordConfig = new DiscordConfiguration
        {
            Token = botSettings.Token ?? "fake-test-token"
        };
        var mockClient = new DiscordClient(discordConfig);
        var tcs = new TaskCompletionSource<GuildCreateEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);

        mockClient.GuildAvailable += (_, e) =>
        {
            tcs.TrySetResult(e);
            return Task.CompletedTask;
        };

        var loggerMock = new Mock<ILogger<DiscordClientEventHandler>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var guildDataRepositoryMock = new Mock<IGuildDataRepository>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ILavaLinkService)))
            .Returns(new Mock<ILavaLinkService>().Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ILocalizationService)))
            .Returns(new Mock<ILocalizationService>().Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IMusicQueueService)))
            .Returns(new Mock<IMusicQueueService>().Object);

        var handler = new DiscordClientEventHandler(loggerMock.Object, guildDataRepositoryMock.Object,
            serviceProviderMock.Object);

        await mockClient.ConnectAsync();
        var guildArgs = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(30));

        await handler.OnGuildAvailable(mockClient, guildArgs);

        serviceProviderMock.Verify(sp => sp.GetService(typeof(ILavaLinkService)), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(ILocalizationService)), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IMusicQueueService)), Times.Once);

        await mockClient.DisconnectAsync();
    }
}