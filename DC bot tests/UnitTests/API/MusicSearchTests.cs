using System.Net;
using System.Text;
using System.Text.Json;
using API.Responses.MusicSearch;
using API.Services.MusicSearch;
using DC_bot.Music;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DC_bot_tests.UnitTests.API;

[Trait("Category", "Unit")]
public sealed class MusicSearchTests
{
    private const string NodeInfo = """{"sourceManagers":["youtube","spotify","soundcloud"],"plugins":[{"name":"lavasearch-plugin","version":"1.0.0"}]}""";
    private const string Track = """{"info":{"identifier":"abc","title":"Song","author":"Creator","length":125500,"isStream":false,"uri":"https://www.youtube.com/watch?v=abc","artworkUrl":"https://i.ytimg.com/vi/abc/default.jpg"}}""";

    [Fact]
    public async Task Search_EncodesQueryAndRetainsAllDistinctHitsWithSecondsAndExplicitEnqueue()
    {
        using var fixture = new Fixture("ytsearch", "{\"loadType\":\"search\",\"data\":[" + Track + "," + Track.Replace("abc", "def") + "," + Track + "]}");
        var result = await fixture.Service.SearchAsync("ytsearch", "track", "  a & b?  ", null, default);
        Assert.Equal(2, result.Results.Count);
        Assert.Equal(125, result.Results[0].DurationSeconds);
        Assert.True(result.Results[0].CanEnqueue);
        Assert.Null(result.NextPageToken);
        Assert.True(result.IsLimited);
        Assert.Equal("?identifier=ytsearch%3Aa%20%26%20b%3F", fixture.Handler.Requests[1].Query);
        Assert.All(fixture.Handler.Methods, method => Assert.Equal(HttpMethod.Get, method));
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Contains("\"results\":", json);
        Assert.Contains("\"durationSeconds\":125", json);
        Assert.DoesNotContain("encoded", json);
    }

    [Theory]
    [InlineData("sptfy", "spsearch")]
    [InlineData("ytmsearch", "ytmsearch")]
    public async Task Search_TranslatesWireProviderWithoutChangingIdentity(string provider, string prefix)
    {
        using var fixture = new Fixture(provider, """{"loadType":"empty","data":null}""");
        await fixture.Service.SearchAsync(provider, "track", "music", null, default);
        Assert.Equal($"?identifier={prefix}%3Amusic", fixture.Handler.Requests[1].Query);
    }

    [Fact]
    public async Task PlaylistSearch_UsesMetadataWithoutExpandingEmptyTracks()
    {
        using var fixture = new Fixture("sptfy", """
            {"playlists":[{"info":{"name":"Mix"},"pluginInfo":{"url":"https://open.spotify.com/playlist/abc","author":"Owner","totalTracks":42},"tracks":[]}]}
            """);
        fixture.Options.Providers["sptfy"].PlaylistSearch = true;
        var page = await fixture.Service.SearchAsync("sptfy", "playlist", "mix", null, default);
        var item = Assert.Single(page.Results);
        Assert.Equal("playlist", item.Kind);
        Assert.Equal(42, item.ItemCount);
        Assert.Null(item.DurationSeconds);
        Assert.Equal("Owner", item.Creator);
        Assert.Equal("/v4/loadsearch", fixture.Handler.Requests[1].AbsolutePath);
        Assert.EndsWith("&types=playlist", fixture.Handler.Requests[1].Query);
        Assert.Equal(2, fixture.Handler.Requests.Count);
    }

    [Theory]
    [InlineData("unknown", "track", "song", null, "InvalidProvider")]
    [InlineData("ytsearch", "album", "song", null, "InvalidKind")]
    [InlineData("ytsearch", "track", " ", null, "InvalidQuery")]
    [InlineData("ytsearch", "track", "song", "forged", "InvalidPageToken")]
    [InlineData("ytsearch", "track", "song", "", "InvalidPageToken")]
    [InlineData("amsearch", "track", "song", null, "ProviderDisabled")]
    public async Task InvalidRequests_DoNotContactUpstream(string provider, string kind, string query, string? token, string code)
    {
        using var fixture = new Fixture("ytsearch", "{}");
        var error = await Assert.ThrowsAsync<MusicSearchException>(() => fixture.Service.SearchAsync(provider, kind, query, token, default));
        Assert.Equal(code, error.Code);
        Assert.Empty(fixture.Handler.Requests);
    }

    [Fact]
    public async Task Capabilities_ReportAllProvidersAndRespectSourcesAndRollout()
    {
        using var fixture = new Fixture("ytsearch", "{}");
        fixture.Options.Providers["amsearch"] = new() { Enabled = true, CanEnqueue = true };
        var result = await fixture.Service.GetCapabilitiesAsync(default);
        Assert.Equal(8, result.Count);
        Assert.True(result.Single(p => p.ProviderId == "ytsearch").SupportsTrackSearch);
        Assert.False(result.Single(p => p.ProviderId == "ytsearch").SupportsPlaylistSearch);
        Assert.Equal("SourceUnavailable", result.Single(p => p.ProviderId == "amsearch").UnavailableReason);
        Assert.False(result.Single(p => p.ProviderId == "sptfy").CanEnqueue);
        await fixture.Service.GetCapabilitiesAsync(default);
        Assert.Single(fixture.Handler.Requests); // only node readiness is cached, never search metadata
    }

    [Fact]
    public async Task UnsupportedPlaylist_IsDistinctFromEmptyResults()
    {
        using var fixture = new Fixture("scsearch", "{}");
        fixture.Options.Providers["scsearch"].PlaylistSearch = true;
        var error = await Assert.ThrowsAsync<MusicSearchException>(() => fixture.Service.SearchAsync("scsearch", "playlist", "music", null, default));
        Assert.Equal("UnsupportedKind", error.Code);
        Assert.Single(fixture.Handler.Requests);
    }

    [Theory]
    [InlineData("{\"loadType\":\"error\",\"data\":{\"message\":\"SECRET\"}}", "ProviderFailure")]
    [InlineData("{\"loadType\":\"playlist\",\"data\":{}}", "InvalidProviderResponse")]
    [InlineData("{\"loadType\":\"search\",\"data\":{}}", "InvalidProviderResponse")]
    [InlineData("not json", "InvalidProviderResponse")]
    public async Task UpstreamFailures_AreNotEmptySuccessOrSecretLeak(string body, string code)
    {
        using var fixture = new Fixture("ytsearch", body);
        var error = await Assert.ThrowsAsync<MusicSearchException>(() => fixture.Service.SearchAsync("ytsearch", "track", "song", null, default));
        Assert.Equal(code, error.Code);
        Assert.DoesNotContain("SECRET", error.Message);
    }

    [Theory]
    [InlineData(429, "ProviderRateLimited", 429)]
    [InlineData(401, "ProviderUnavailable", 503)]
    [InlineData(500, "ProviderFailure", 502)]
    public async Task HttpFailures_HaveStableCodes(int status, string code, int expectedStatus)
    {
        using var fixture = new Fixture("ytsearch", "SECRET", (HttpStatusCode)status);
        var error = await Assert.ThrowsAsync<MusicSearchException>(() => fixture.Service.SearchAsync("ytsearch", "track", "song", null, default));
        Assert.Equal(code, error.Code);
        Assert.Equal(expectedStatus, error.StatusCode);
        if (status == 429) Assert.Equal(30, error.RetryAfterSeconds);
    }

    [Fact]
    public async Task NoContent_ReturnsEmptyPage()
    {
        using var fixture = new Fixture("ytsearch", "", HttpStatusCode.NoContent);
        Assert.Empty((await fixture.Service.SearchAsync("ytsearch", "track", "song", null, default)).Results);
    }

    [Fact]
    public async Task CallerCancellation_Propagates()
    {
        using var fixture = new Fixture("ytsearch", "{}");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => fixture.Service.SearchAsync("ytsearch", "track", "song", null, cancellation.Token));
    }

    [Fact]
    public void Normalization_PreservesUnknownMetadataAndRejectsWrongProviderUrls()
    {
        using var document = JsonDocument.Parse("""
            {"loadType":"search","data":[{"info":{"identifier":"live","title":"Live","isStream":true,"length":0,"uri":"http://127.0.0.1/admin","artworkUrl":"https://127.0.0.1/image"}}]}
            """);
        var result = Assert.Single(MusicSearchResultMapper.Map(document.RootElement, MusicSearchProviders.Find("ytsearch")!, "track", true, 20).Results);
        Assert.Null(result.DurationSeconds);
        Assert.Null(result.CanonicalUrl);
        Assert.Null(result.ThumbnailUrl);
        Assert.Null(result.Creator);
        Assert.False(result.CanEnqueue);
    }

    [Fact]
    public void ResultLimitAndEnqueuePolicy_AreEnforced()
    {
        using var document = JsonDocument.Parse("{\"loadType\":\"search\",\"data\":[" + Track + "," + Track.Replace("abc", "def") + "]}");
        var result = Assert.Single(MusicSearchResultMapper.Map(document.RootElement, MusicSearchProviders.Find("ytsearch")!, "track", false, 1).Results);
        Assert.False(result.CanEnqueue);
        Assert.NotNull(result.CanonicalUrl);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly MemoryCache cache = new(new MemoryCacheOptions());
        private readonly MusicSearchQuota quota = new();
        private readonly HttpClient http;
        public MusicSearchOptions Options { get; } = new() { Password = "test-password" };
        public StubHandler Handler { get; }
        public MusicSearchService Service { get; }

        public Fixture(string provider, string body, HttpStatusCode status = HttpStatusCode.OK)
        {
            Options.Providers[provider] = new() { Enabled = true, CanEnqueue = true };
            Handler = new StubHandler(body, status);
            http = new HttpClient(Handler) { BaseAddress = new Uri("http://lavalink.test/") };
            var options = Microsoft.Extensions.Options.Options.Create(Options);
            Service = new MusicSearchService(new LavalinkSearchClient(http, options), options, cache, quota);
        }

        public void Dispose() { http.Dispose(); cache.Dispose(); quota.Dispose(); }
    }

    private sealed class StubHandler(string body, HttpStatusCode status) : HttpMessageHandler
    {
        public List<Uri> Requests { get; } = [];
        public List<HttpMethod> Methods { get; } = [];
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request.RequestUri!);
            Methods.Add(request.Method);
            Assert.Equal("test-password", request.Headers.GetValues("Authorization").Single());
            var info = request.RequestUri!.AbsolutePath == "/v4/info";
            return Task.FromResult(new HttpResponseMessage(info ? HttpStatusCode.OK : status)
            {
                Content = new StringContent(info ? NodeInfo : body, Encoding.UTF8, "application/json")
            });
        }
    }
}
