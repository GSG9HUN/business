using DC_bot_tests.TestHelperFiles;
using DC_bot.Constants;
using DC_bot.Exceptions.Messaging;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Service.Music.MusicServices;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

[Trait("Category", "Unit")]
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
        channel.Setup(x => x.SendMessageAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("send fail"));

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
        var guild = new Mock<IDiscordGuild>();

        localization.Setup(x => x.Get(LocalizationKeys.SkipCommandQueueIsEmpty)).Returns("Queue empty");
        localization.Setup(x => x.Get(It.IsAny<ulong>(), LocalizationKeys.SkipCommandQueueIsEmpty))
            .Returns("Queue empty");
        guild.SetupGet(x => x.Id).Returns(123UL);
        channel.SetupGet(x => x.Guild).Returns(guild.Object);
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
        var guild = new Mock<IDiscordGuild>();

        localization.Setup(x => x.Get(LocalizationKeys.PlayCommandMusicPlaying)).Returns("Now playing: ");
        localization.Setup(x => x.Get(It.IsAny<ulong>(), LocalizationKeys.PlayCommandMusicPlaying))
            .Returns("Now playing: ");
        guild.SetupGet(x => x.Id).Returns(123UL);
        channel.SetupGet(x => x.Guild).Returns(guild.Object);

        var service = new TrackNotificationService(localization.Object, logger.Object, client);
        var raised = false;
        service.TrackStarted += (_, _, msg) =>
        {
            raised = msg.Description.Contains("Artist") && msg.Description.Contains("Title");
            return Task.CompletedTask;
        };

        var track = TrackTestHelper.CreateTrackWrapper("Artist", "Title");

        await service.NotifyNowPlayingAsync(channel.Object, track, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(120));

        Assert.True(raised);
    }

    [Fact]
    public async Task NotifyNowPlayingAsync_WithoutTrackStartedSubscribers_DoesNotThrow()
    {
        var localization = new Mock<ILocalizationService>();
        var logger = new Mock<ILogger<TrackNotificationService>>();
        var client = new DiscordClient(new DiscordConfiguration { Token = "x", TokenType = TokenType.Bot });
        var channel = new Mock<IDiscordChannel>();

        localization.Setup(x => x.Get(LocalizationKeys.PlayCommandMusicPlaying)).Returns("Now playing: ");

        var service = new TrackNotificationService(localization.Object, logger.Object, client);
        var track = TrackTestHelper.CreateTrackWrapper("Artist", "Title");

        await service.NotifyNowPlayingAsync(channel.Object, track, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(120));
    }

    [Fact]
    public void BuildNowPlayingEmbed_WithArtwork_SetsThumbnail()
    {
        var localization = new Mock<ILocalizationService>();
        var logger = new Mock<ILogger<TrackNotificationService>>();
        var client = new DiscordClient(new DiscordConfiguration { Token = "x", TokenType = TokenType.Bot });
        localization.Setup(x => x.Get(LocalizationKeys.PlayCommandMusicPlaying)).Returns("Now playing:");

        var service = new TrackNotificationService(localization.Object, logger.Object, client);
        var track = new Mock<ILavaLinkTrack>();
        track.SetupGet(t => t.Author).Returns("Artist");
        track.SetupGet(t => t.Title).Returns("Title");
        track.SetupGet(t => t.ArtworkUri).Returns(new Uri("https://example.com/art.jpg"));

        var embed = service.BuildNowPlayingEmbed(track.Object, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(120));

        Assert.NotNull(embed.Thumbnail);
        Assert.Contains("Artist", embed.Description);
        Assert.Contains("Title", embed.Description);
    }

    [Fact]
    public void BuildNowPlayingEmbed_WithoutArtwork_DoesNotSetThumbnail()
    {
        var localization = new Mock<ILocalizationService>();
        var logger = new Mock<ILogger<TrackNotificationService>>();
        var client = new DiscordClient(new DiscordConfiguration { Token = "x", TokenType = TokenType.Bot });
        localization.Setup(x => x.Get(LocalizationKeys.PlayCommandMusicPlaying)).Returns("Now playing:");

        var service = new TrackNotificationService(localization.Object, logger.Object, client);
        var track = new Mock<ILavaLinkTrack>();
        track.SetupGet(t => t.Author).Returns("Artist");
        track.SetupGet(t => t.Title).Returns("Title");
        track.SetupGet(t => t.ArtworkUri).Returns((Uri?)null);

        var embed = service.BuildNowPlayingEmbed(track.Object, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(100));

        Assert.Null(embed.Thumbnail);
        Assert.Contains("00:00 / 01:40", embed.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void BuildNowPlayingEmbed_WithNonPositiveDuration_ClampsToZeroDuration(int durationSeconds)
    {
        var localization = new Mock<ILocalizationService>();
        var logger = new Mock<ILogger<TrackNotificationService>>();
        var client = new DiscordClient(new DiscordConfiguration { Token = "x", TokenType = TokenType.Bot });
        localization.Setup(x => x.Get(LocalizationKeys.PlayCommandMusicPlaying)).Returns("Now playing:");

        var service = new TrackNotificationService(localization.Object, logger.Object, client);
        var track = new Mock<ILavaLinkTrack>();
        track.SetupGet(t => t.Author).Returns("Artist");
        track.SetupGet(t => t.Title).Returns("Title");

        var embed = service.BuildNowPlayingEmbed(track.Object, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(durationSeconds));

        Assert.Contains("00:00 / 00:00", embed.Description);
    }

    [Fact]
    public void BuildNowPlayingEmbed_WhenPositionExceedsDuration_ClampsPositionToDuration()
    {
        var localization = new Mock<ILocalizationService>();
        var logger = new Mock<ILogger<TrackNotificationService>>();
        var client = new DiscordClient(new DiscordConfiguration { Token = "x", TokenType = TokenType.Bot });
        localization.Setup(x => x.Get(LocalizationKeys.PlayCommandMusicPlaying)).Returns("Now playing:");

        var service = new TrackNotificationService(localization.Object, logger.Object, client);
        var track = new Mock<ILavaLinkTrack>();
        track.SetupGet(t => t.Author).Returns("Artist");
        track.SetupGet(t => t.Title).Returns("Title");

        var embed = service.BuildNowPlayingEmbed(track.Object, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(2));

        Assert.Contains("02:00 / 02:00", embed.Description);
    }
}
