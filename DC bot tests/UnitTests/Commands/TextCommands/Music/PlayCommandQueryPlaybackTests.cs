using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Music;

[Trait("Category", "Unit")]
public class PlayCommandQueryPlaybackTests : PlayCommandTestBase
{
    [Fact]
    public async Task ExecuteAsync_UserProvidedTitle_ShouldCall_PlayAsyncQuery_With_Youtube_Search_Mode()
    {
        ArrangeValidVoiceRequest(PlayCommandContentSearch);

        await PlayCommand.ExecuteAsync(MessageMock.Object);

        VerifyQueryPlayback(TrackSearchMode.YouTube);
    }

    public static IEnumerable<object[]> SearchPrefixCases()
    {
        yield return [PlayCommandContentSpotifySearch, TrackSearchMode.Spotify];
        yield return [PlayCommandContentSoundCloudSearch, TrackSearchMode.SoundCloud];
        yield return [PlayCommandContentYouTubeSearch, TrackSearchMode.YouTube];
        yield return [PlayCommandContentYouTubeMusicSearch, TrackSearchMode.YouTubeMusic];
    }

    [Theory]
    [MemberData(nameof(SearchPrefixCases))]
    public async Task ExecuteAsync_UserProvidedSearchPrefix_ShouldCall_PlayAsyncUrl_WithExpectedSearchMode(
        string content,
        TrackSearchMode expectedSearchMode)
    {
        ArrangeValidVoiceRequest(content);

        await PlayCommand.ExecuteAsync(MessageMock.Object);

        VerifyUrlPlayback(expectedSearchMode);
    }
}
