using DC_bot.Configuration;
using DC_bot.Service.BotControl;
using DC_bot.Service.Music;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;

namespace DC_bot_tests.UnitTests.Service.BotControl;

[Trait("Category", "Unit")]
public sealed class MobilePlayInputTests
{
    private readonly TrackSearchResolverService resolver = new(Options.Create(new SearchResolverOptions()));

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=abc")]
    [InlineData("https://www.youtube.com/playlist?list=abc")]
    [InlineData("https://open.spotify.com/track/abc")]
    public void CanonicalUrl_IsPreservedAndNeverResearched(string url)
    {
        Assert.True(MobilePlayInput.TryParse(url, "YouTube", resolver, out var input));
        Assert.Equal(url, input!.Url!.OriginalString);
        Assert.Equal(TrackSearchMode.None, input.SearchMode);
    }

    public static IEnumerable<object[]> QueryCases => new[]
    {
        new object[] { "sptfy: song name", TrackSearchMode.Spotify },
        new object[] { "spotify: song name", TrackSearchMode.Spotify },
        new object[] { "ytmsearch: song name", TrackSearchMode.YouTubeMusic },
        new object[] { "song name", TrackSearchMode.YouTube }
    };

    [Theory]
    [MemberData(nameof(QueryCases))]
    public void ManualQueries_KeepProviderAndRemoveInputPrefix(string value, TrackSearchMode expected)
    {
        Assert.True(MobilePlayInput.TryParse(value, null, resolver, out var input));
        Assert.Equal("song name", input!.Query);
        Assert.Equal(expected, input.SearchMode);
        Assert.Null(input.Url);
    }

    [Theory]
    [InlineData("http://127.0.0.1:2333/", null)]
    [InlineData("file:///etc/passwd", null)]
    [InlineData("https://youtube.com.evil.test/track", null)]
    [InlineData("https://evil.test@youtube.com/watch?v=abc", null)]
    [InlineData("song", "invalid-mode")]
    [InlineData("song", "1")]
    [InlineData("ytsearch:", null)]
    public void UnsafeOrInvalidInput_IsRejected(string input, string? mode) =>
        Assert.False(MobilePlayInput.TryParse(input, mode, resolver, out _));
}
