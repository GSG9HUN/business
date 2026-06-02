using DC_bot.Configuration;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
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
        var guildDataRepositoryMock = new Mock<IGuildDataRepository>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var eventHandler = new DiscordClientEventHandler(loggerMock.Object, guildDataRepositoryMock.Object,
            localizationServiceMock.Object, lavaLinkServiceMock.Object);

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
    public async Task OnGuildAvailable_WithNullArgs_DoesNotCallDependencies()
    {
        var loggerMock = new Mock<ILogger<DiscordClientEventHandler>>();
        var guildDataRepositoryMock = new Mock<IGuildDataRepository>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var eventHandler = new DiscordClientEventHandler(loggerMock.Object, guildDataRepositoryMock.Object,
            localizationServiceMock.Object, lavaLinkServiceMock.Object);

        await eventHandler.OnGuildAvailable(null!, null!);

        guildDataRepositoryMock.Verify(
            repository => repository.EnsureGuildExistsAsync(It.IsAny<ulong>(), It.IsAny<CancellationToken>()),
            Times.Never);
        localizationServiceMock.Verify(service => service.LoadLanguage(It.IsAny<ulong>()), Times.Never);
        lavaLinkServiceMock.Verify(service => service.Init(It.IsAny<ulong>()), Times.Never);
    }

    [Fact]
    public async Task OnGuildAvailable_CallsStartupDependencies()
    {
        if (!EndToEndTestConfiguration.TryGetDiscordToken(out var envToken))
        {
            throw SkipException.ForSkip(EndToEndTestConfiguration.MissingDiscordTokenMessage());
        }

        var botSettings = new BotSettings { Token = envToken, Prefix = "!" };

        var tcs = new TaskCompletionSource<GuildAvailableEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);

        var mockClient = DiscordClientBuilder
            .CreateDefault(botSettings.Token ?? "fake-test-token", DiscordIntents.All)
            .ConfigureEventHandlers(builder =>
            {
                builder.HandleGuildAvailable((_, e) =>
                {
                    tcs.TrySetResult(e);
                    return Task.CompletedTask;
                });
            })
            .Build();

        var loggerMock = new Mock<ILogger<DiscordClientEventHandler>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var guildDataRepositoryMock = new Mock<IGuildDataRepository>();
        var localizationServiceMock = new Mock<ILocalizationService>();
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();

        var handler = new DiscordClientEventHandler(loggerMock.Object, guildDataRepositoryMock.Object,
            localizationServiceMock.Object, lavaLinkServiceMock.Object);

        await mockClient.ConnectAsync();
        var guildArgs = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(30));

        await handler.OnGuildAvailable(mockClient, guildArgs);

        guildDataRepositoryMock.Verify(
            repository => repository.EnsureGuildExistsAsync(guildArgs.Guild.Id, CancellationToken.None),
            Times.Once);
        localizationServiceMock.Verify(service => service.LoadLanguage(guildArgs.Guild.Id), Times.Once);
        lavaLinkServiceMock.Verify(service => service.Init(guildArgs.Guild.Id), Times.Once);

        await mockClient.DisconnectAsync();
    }
}
