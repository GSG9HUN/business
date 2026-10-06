using API.Requests.Queue;
using API.Services.MusicSearch;
using API.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DC_bot_tests.UnitTests.API;

[Trait("Category", "Unit")]
public sealed class MusicEnqueueValidationTests
{
    [Theory]
    [InlineData("https://open.spotify.com/track/abc", null, 422)]
    [InlineData("sptfy:https://open.spotify.com/track/abc", null, 422)]
    [InlineData("spotify:music", null, 422)]
    [InlineData("music", "Spotify", 422)]
    [InlineData("http://127.0.0.1:2333/v4/info", null, 400)]
    [InlineData("https://www.youtube.com/redirect?q=http://localhost", null, 400)]
    [InlineData("file:///secret", null, 400)]
    public async Task InvalidOrDisabledProvider_NeverReachesCommandCreation(string query, string? mode, int expected)
    {
        var filter = CreateFilter();
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), new EnqueueRequest(query, mode));
        var called = false;
        var result = await filter.InvokeAsync(context, _ => { called = true; return ValueTask.FromResult<object?>(null); });
        Assert.False(called);
        Assert.Equal(expected, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
    }

    [Theory]
    [InlineData("a song")]
    [InlineData("ytsearch:a song")]
    [InlineData("youtubemusic:a song")]
    [InlineData("https://www.youtube.com/watch?v=abc")]
    [InlineData("https://www.youtube.com/playlist?list=abc")]
    public async Task AllowedManualAndSelectedInput_ContinueUnchanged(string query)
    {
        var request = new EnqueueRequest(query);
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), request);
        var result = await CreateFilter().InvokeAsync(context, _ => ValueTask.FromResult<object?>(request));
        Assert.Same(request, result);
        Assert.Equal(query, request.Query);
    }

    private static MusicEnqueueValidationFilter CreateFilter() => new(Options.Create(new MusicSearchOptions
    {
        Providers = new()
        {
            ["ytsearch"] = new() { CanEnqueue = true },
            ["ytmsearch"] = new() { CanEnqueue = true }
        }
    }));
}
