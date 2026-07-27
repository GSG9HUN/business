using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Exceptions.Messaging;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Service.ReactionHandler;
using DC_bot.Startup.DependencyInjection;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service.ReactionHandler;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class ReactionHandlerDependencyInjectionIntegrationTests
{
    [Fact]
    public async Task AddCoreBotServices_RegistersReactionComponentsAsSingletons()
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        await using var provider = CreateServiceProvider(lavaLinkServiceMock);

        Assert.Same(
            provider.GetRequiredService<ReactionActionDispatcher>(),
            provider.GetRequiredService<ReactionActionDispatcher>());
        Assert.Same(
            provider.GetRequiredService<ReactionContextFactory>(),
            provider.GetRequiredService<ReactionContextFactory>());
        Assert.Same(
            provider.GetRequiredService<ReactionControlMessageService>(),
            provider.GetRequiredService<ReactionControlMessageService>());
        Assert.Same(
            provider.GetRequiredService<ReactionHandlerService>(),
            provider.GetRequiredService<ReactionHandlerService>());
    }

    [Fact]
    public async Task RegisteredReactionHandler_WhenTrackStartedRaised_DelegatesToControlMessageService()
    {
        var lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var progressiveTimerServiceMock = new Mock<IProgressiveTimerService>();
        var localizationServiceMock = CreateLocalizationServiceMock();
        await using var provider = CreateServiceProvider(
            lavaLinkServiceMock,
            progressiveTimerServiceMock,
            localizationServiceMock);

        var handler = provider.GetRequiredService<ReactionHandlerService>();
        var client = TestDiscordClientFactory.Create("test-token");
        var channelMock = CreateFailingChannelMock();

        try
        {
            handler.RegisterHandler(client);

            var exception = await Assert.ThrowsAsync<MessageSendException>(
                () => lavaLinkServiceMock.RaiseAsync(
                    service => service.TrackStarted += null!,
                    channelMock.Object,
                    new DiscordEmbedBuilder().WithTitle("track").Build()));

            Assert.Equal("SendReactionControlMessage", exception.Operation);
            channelMock.Verify(channel => channel.ToDiscordChannel(), Times.Once);
            progressiveTimerServiceMock.Verify(
                timer => timer.StartAsync(It.IsAny<IDiscordMessage>(), It.IsAny<ulong>()),
                Times.Never);
        }
        finally
        {
            handler.UnregisterHandler(client);
            DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(client);
        }
    }

    private static ServiceProvider CreateServiceProvider(
        Mock<ILavaLinkService> lavaLinkServiceMock,
        Mock<IProgressiveTimerService>? progressiveTimerServiceMock = null,
        Mock<ILocalizationService>? localizationServiceMock = null)
    {
        progressiveTimerServiceMock ??= new Mock<IProgressiveTimerService>();
        localizationServiceMock ??= CreateLocalizationServiceMock();

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(lavaLinkServiceMock.Object)
            .AddSingleton(progressiveTimerServiceMock.Object)
            .AddCoreBotServices(new BotSettings { Token = "test-token", Prefix = "!" });

        services.AddSingleton(localizationServiceMock.Object);

        return services.BuildServiceProvider();
    }

    private static Mock<IDiscordChannel> CreateFailingChannelMock()
    {
        var channelMock = new Mock<IDiscordChannel>();
        var guildMock = new Mock<IDiscordGuild>();

        guildMock.SetupGet(guild => guild.Id).Returns(123UL);
        channelMock.SetupGet(channel => channel.Guild).Returns(guildMock.Object);
        channelMock.Setup(channel => channel.ToDiscordChannel())
            .Throws(new InvalidOperationException("Discord channel unavailable"));

        return channelMock;
    }

    private static Mock<ILocalizationService> CreateLocalizationServiceMock()
    {
        var localizationServiceMock = new Mock<ILocalizationService>();

        localizationServiceMock
            .Setup(service => service.Get(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<ulong, string, object[]>((_, key, _) => key switch
            {
                LocalizationKeys.MusicControl => "Music control",
                LocalizationKeys.PauseReaction => "Pause",
                LocalizationKeys.ResumeReaction => "Resume",
                LocalizationKeys.SkipReaction => "Skip",
                LocalizationKeys.RepeatReaction => "Repeat",
                _ => key
            });
        localizationServiceMock
            .Setup(service => service.Get(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<string, object[]>((key, _) => key);

        return localizationServiceMock;
    }
}
