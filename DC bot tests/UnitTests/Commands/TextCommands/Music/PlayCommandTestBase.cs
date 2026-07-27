using DC_bot.Commands.TextCommands.Music;
using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using DC_bot.Service.Music;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Music;

public abstract class PlayCommandTestBase
{
    protected const string PlayCommandName = "play";
    protected const string PlayCommandDescriptionValue = "Start playing a music.";
    protected const string PlayCommandContentNoArgs = "!play";
    protected const string PlayCommandContentYouTube = "!play https://www.youtube.com/watch?v=zIRszCXKzGc";
    protected const string PlayCommandContentSoundCloud = "!play https://soundcloud.com/madeon/imperium";
    protected const string PlayCommandContentSpotify = "!play https://open.spotify.com/track/0DI3WNmIyfi2fS3a15wZDP";
    protected const string PlayCommandContentAppleMusic =
        "!play https://music.apple.com/us/album/bad-guy/1450695723?i=1450695739";
    protected const string PlayCommandContentDeezer = "!play https://www.deezer.com/track/3135556";
    protected const string PlayCommandContentYandex = "!play https://music.yandex.ru/album/12345/track/67890";
    protected const string PlayCommandContentYouTubeMusic = "!play https://music.youtube.com/watch?v=zIRszCXKzGc";
    protected const string PlayCommandContentSearch = "!play madeon imperium";
    protected const string PlayCommandContentSpotifySearch = "!play sptfy:madeon imperium";
    protected const string PlayCommandContentSoundCloudSearch = "!play scsearch:madeon imperium";
    protected const string PlayCommandContentYouTubeSearch = "!play ytsearch:madeon imperium";
    protected const string PlayCommandContentYouTubeMusicSearch = "!play ytmsearch:madeon imperium";

    protected PlayCommandTestBase()
    {
        var loggerMock = new Mock<ILogger<PlayCommand>>();
        var localizationServiceMock = new Mock<ILocalizationService>();

        localizationServiceMock.Setup(g => g.Get(LocalizationKeys.PlayCommandDescription))
            .Returns(PlayCommandDescriptionValue);

        CommandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new UserValidationResult(true, string.Empty, DiscordMemberMock.Object));

        CommandHelperMock
            .Setup(h => h.TryGetArgumentAsync(
                It.IsAny<IDiscordMessage>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<ILogger>(),
                It.IsAny<string>()))
            .Returns<IDiscordMessage, IResponseBuilder, ILogger, string>(async (msg, rb, _, commandName) =>
            {
                var parts = msg.Content.Split(" ", 2);
                if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1])) return parts[1].Trim();
                await rb.SendUsageAsync(msg, commandName);
                return null;
            });

        var userValidationService = new ValidationService(new Mock<ILogger<ValidationService>>().Object);
        var trackSearchResolverService = new TrackSearchResolverService(
            Options.Create(new SearchResolverOptions { DefaultQueryMode = "yt" }));

        PlayCommand = new PlayCommand(
            LavaLinkServiceMock.Object,
            userValidationService,
            ResponseBuilderMock.Object,
            loggerMock.Object,
            trackSearchResolverService,
            localizationServiceMock.Object,
            CommandHelperMock.Object);
    }

    protected Mock<ICommandHelper> CommandHelperMock { get; } = new();
    protected Mock<IDiscordChannel> ChannelMock { get; } = new();
    protected Mock<IDiscordMember> DiscordMemberMock { get; } = new();
    protected Mock<IDiscordUser> DiscordUserMock { get; } = new();
    protected Mock<IDiscordGuild> GuildMock { get; } = new();
    protected Mock<ILavaLinkService> LavaLinkServiceMock { get; } = new();
    protected Mock<IDiscordMessage> MessageMock { get; } = new();
    protected PlayCommand PlayCommand { get; }
    protected Mock<IResponseBuilder> ResponseBuilderMock { get; } = new();

    protected void ArrangeValidVoiceRequest(string content)
    {
        var discordVoiceStateMock = new Mock<IDiscordVoiceState>();
        discordVoiceStateMock.Setup(vs => vs.Channel).Returns(ChannelMock.Object);

        DiscordUserMock.Setup(du => du.Id).Returns(1564123UL);
        DiscordMemberMock.Setup(dm => dm.IsBot).Returns(false);
        DiscordMemberMock.SetupGet(dm => dm.VoiceState).Returns(discordVoiceStateMock.Object);
        GuildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(DiscordMemberMock.Object);
        ChannelMock.SetupGet(c => c.Guild).Returns(GuildMock.Object);
        MessageMock.SetupGet(m => m.Author).Returns(DiscordUserMock.Object);
        MessageMock.Setup(m => m.Channel).Returns(ChannelMock.Object);
        MessageMock.SetupGet(m => m.Content).Returns(content);
    }

    protected void ArrangeValidationFailure()
    {
        CommandHelperMock.Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync((UserValidationResult?)null);
    }

    protected void ArrangeArgument(string argument)
    {
        CommandHelperMock.Setup(h => h.TryGetArgumentAsync(
                It.IsAny<IDiscordMessage>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<ILogger>(),
                It.IsAny<string>()))
            .ReturnsAsync(argument);
    }

    protected void VerifyNoPlayback()
    {
        LavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordMessage>(),
                It.IsAny<TrackSearchMode>()),
            Times.Never);
        LavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordMessage>(),
                It.IsAny<TrackSearchMode>()),
            Times.Never);
    }

    protected void VerifyUrlPlayback(TrackSearchMode trackSearchMode)
    {
        LavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordMessage>(),
                trackSearchMode),
            Times.Never);
        LavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordMessage>(),
                trackSearchMode),
            Times.Once);
    }

    protected void VerifyQueryPlayback(TrackSearchMode trackSearchMode)
    {
        LavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordMessage>(),
                trackSearchMode),
            Times.Once);
        LavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordMessage>(),
                trackSearchMode),
            Times.Never);
    }
}
