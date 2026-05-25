using DC_bot.Configuration;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Sdk;

namespace DC_bot_tests.EndToEndTests.Wrapper;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class DiscordClientEventHandlerEndToEndTests
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
    }

    [Fact]
    public async Task OnGuildAvailable_Call_GetRequiredService_Two_Times()
    {
        if (!EndToEndTestConfiguration.TryGetDiscordToken(out var envToken))
        {
            throw SkipException.ForSkip(EndToEndTestConfiguration.MissingDiscordTokenMessage());
        }

        var botSettings = new BotSettings { Token = envToken, Prefix = "!" };

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

        await mockClient.DisconnectAsync();
    }
}
