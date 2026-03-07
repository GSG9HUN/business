using DC_bot.Constants;
using DC_bot.Exceptions.Messaging;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Service.Music.MusicServices;
using DSharpPlus;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

public class TrackNotificationServiceTests
{
    [Fact]
    public async Task SendSafeAsync_WhenSendSucceeds_DoesNotThrow()
    {
        var localization = new Mock<ILocalizationService>();
        var logger = new Mock<ILogger<TrackNotificationService>>();
        var client = new DiscordClient(new DiscordConfiguration { Token = "x", TokenType = TokenType.Bot });
        var channel = new Mock<IDiscordChannel>();
        channel.Setup(x => x.SendMessageAsync("msg")).Returns(Task.CompletedTask);

        var service = new TrackNotificationService(localization.Object, logger.Object, client);

        await service.SendSafeAsync(channel.Object, "msg", "op");

        channel.Verify(x => x.SendMessageAsync("msg"), Times.Once);
    }

    [Fact]
    public async Task SendSafeAsync_WhenSendFails_ThrowsMessageSendException()
    {
        var localization = new Mock<ILocalizationService>();
        var logger = new Mock<ILogger<TrackNotificationService>>();
        var client = new DiscordClient(new DiscordConfiguration { Token = "x", TokenType = TokenType.Bot });
        var channel = new Mock<IDiscordChannel>();
        channel.Setup(x => x.SendMessageAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("send fail"));

        var service = new TrackNotificationService(localization.Object, logger.Object, client);

        await Assert.ThrowsAsync<MessageSendException>(() => service.SendSafeAsync(channel.Object, "msg", "op"));
    }

    [Fact]
    public async Task NotifyQueueEmptyAsync_SendsLocalizedMessage()
    {
        var localization = new Mock<ILocalizationService>();
        var logger = new Mock<ILogger<TrackNotificationService>>();
        var client = new DiscordClient(new DiscordConfiguration { Token = "x", TokenType = TokenType.Bot });
        var channel = new Mock<IDiscordChannel>();

        localization.Setup(x => x.Get(LocalizationKeys.SkipCommandQueueIsEmpty)).Returns("Queue empty");
        channel.Setup(x => x.SendMessageAsync("Queue empty")).Returns(Task.CompletedTask);

        var service = new TrackNotificationService(localization.Object, logger.Object, client);

        await service.NotifyQueueEmptyAsync(channel.Object);

        channel.Verify(x => x.SendMessageAsync("Queue empty"), Times.Once);
    }

    [Fact]
    public async Task NotifyNowPlayingAsync_RaisesTrackStartedEvent()
    {
        var localization = new Mock<ILocalizationService>();
        var logger = new Mock<ILogger<TrackNotificationService>>();
        var client = new DiscordClient(new DiscordConfiguration { Token = "x", TokenType = TokenType.Bot });
        var channel = new Mock<IDiscordChannel>();

        localization.Setup(x => x.Get(LocalizationKeys.PlayCommandMusicPlaying)).Returns("Now playing: ");

        var service = new TrackNotificationService(localization.Object, logger.Object, client);
        var raised = false;
        service.TrackStarted += (_, _, msg) =>
        {
            raised = msg.Contains("Now playing:") && msg.Contains("Artist") && msg.Contains("Title");
            return Task.CompletedTask;
        };

        var track = new LavalinkTrack { Author = "Artist", Title = "Title", Identifier = "id" };

        await service.NotifyNowPlayingAsync(channel.Object, track);

        Assert.True(raised);
    }
}