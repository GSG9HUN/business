using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Music;

[Trait("Category", "Unit")]
public class PlayCommandUrlPlaybackTests : PlayCommandTestBase
{
    public static IEnumerable<object[]> UrlPlaybackCases()
    {
        yield return [PlayCommandContentYouTube, TrackSearchMode.YouTube];
        yield return [PlayCommandContentSoundCloud, TrackSearchMode.SoundCloud];
        yield return [PlayCommandContentSpotify, TrackSearchMode.Spotify];
        yield return [PlayCommandContentAppleMusic, TrackSearchMode.AppleMusic];
        yield return [PlayCommandContentDeezer, TrackSearchMode.Deezer];
        yield return [PlayCommandContentYandex, TrackSearchMode.YandexMusic];
        yield return [PlayCommandContentYouTubeMusic, TrackSearchMode.YouTubeMusic];
    }

    [Theory]
    [MemberData(nameof(UrlPlaybackCases))]
    public async Task ExecuteAsync_UserProvidedURL_ShouldCall_PlayAsyncURL_WithExpectedSearchMode(
        string content,
        TrackSearchMode expectedSearchMode)
    {
        ArrangeValidVoiceRequest(content);

        await PlayCommand.ExecuteAsync(MessageMock.Object);

        VerifyUrlPlayback(expectedSearchMode);
    }
}
