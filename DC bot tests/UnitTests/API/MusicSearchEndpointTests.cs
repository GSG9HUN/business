using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using API.Endpoints;
using API.Responses.MusicSearch;
using API.Services.MusicSearch;
using DC_bot.Interface.Service.Persistence.MobileApps;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DC_bot_tests.UnitTests.API;

[Trait("Category", "Unit")]
public sealed class MusicSearchEndpointTests
{
    [Theory]
    [InlineData("", false, HttpStatusCode.Unauthorized)]
    [InlineData("/capabilities", false, HttpStatusCode.Unauthorized)]
    [InlineData("", true, HttpStatusCode.Forbidden)]
    [InlineData("/capabilities", true, HttpStatusCode.Forbidden)]
    public async Task BothEndpoints_RejectUnauthorizedGuildBeforeDiscovery(string suffix, bool authenticated, HttpStatusCode status)
    {
        await using var fixture = await Server.Create(false);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/guilds/123/music-search{suffix}");
        if (authenticated) request.Headers.Authorization = new("Bearer", "test");
        using var response = await fixture.Client.SendAsync(request);
        Assert.Equal(status, response.StatusCode);
        fixture.Search.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Capabilities_ReturnsArrayWithMobileFieldNames()
    {
        await using var fixture = await Server.Create(true);
        fixture.Search.Setup(s => s.GetCapabilitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new MusicSearchCapabilityResponse("ytsearch", true, false, true, null) });
        fixture.Client.DefaultRequestHeaders.Authorization = new("Bearer", "test");
        using var response = await fixture.Client.GetAsync("/api/guilds/123/music-search/capabilities");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);
        Assert.Equal("ytsearch", json.RootElement[0].GetProperty("providerId").GetString());
        Assert.True(json.RootElement[0].GetProperty("supportsTrackSearch").GetBoolean());
        Assert.True(response.Headers.CacheControl!.NoStore);
    }

    [Fact]
    public async Task Search_ParsesMobileParametersAndReturnsPage()
    {
        await using var fixture = await Server.Create(true);
        fixture.Client.DefaultRequestHeaders.Authorization = new("Bearer", "test");
        fixture.Search.Setup(s => s.SearchAsync("ytsearch", "track", "artist & title", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MusicSearchResponse([]));
        using var response = await fixture.Client.GetAsync("/api/guilds/123/music-search?provider=ytsearch&kind=track&query=artist%20%26%20title");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, json.RootElement.GetProperty("results").ValueKind);
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("nextPageToken").ValueKind);
        fixture.Search.VerifyAll();
    }

    [Theory]
    [InlineData("0", "", HttpStatusCode.BadRequest)]
    [InlineData("not-a-guild", "", HttpStatusCode.BadRequest)]
    [InlineData("123", "?provider=ytsearch&provider=sptfy", HttpStatusCode.BadRequest)]
    public async Task InvalidRouteOrDuplicateParameter_NeverCallsDiscovery(string guild, string query, HttpStatusCode expected)
    {
        await using var fixture = await Server.Create(true);
        fixture.Client.DefaultRequestHeaders.Authorization = new("Bearer", "test");
        using var response = await fixture.Client.GetAsync($"/api/guilds/{guild}/music-search{query}");
        Assert.Equal(expected, response.StatusCode);
        fixture.Search.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProviderQuotaError_ReturnsRetryAfterAndStableErrorBody()
    {
        await using var fixture = await Server.Create(true);
        fixture.Client.DefaultRequestHeaders.Authorization = new("Bearer", "test");
        fixture.Search.Setup(s => s.SearchAsync("ytsearch", "track", "song", null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MusicSearchException("ProviderRateLimited", "Retry later.", 429, 12));
        using var response = await fixture.Client.GetAsync("/api/guilds/123/music-search?provider=ytsearch&kind=track&query=song");
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(12), response.Headers.RetryAfter!.Delta);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("ProviderRateLimited", json.RootElement.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task UserQuota_IsSharedAcrossGuilds()
    {
        await using var fixture = await Server.Create(true);
        fixture.Client.DefaultRequestHeaders.Authorization = new("Bearer", "test");
        fixture.Search.Setup(s => s.GetCapabilitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<MusicSearchCapabilityResponse>());
        for (var i = 0; i < 60; i++)
        {
            using var allowed = await fixture.Client.GetAsync($"/api/guilds/{123 + i}/music-search/capabilities");
            Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        }
        using var rejected = await fixture.Client.GetAsync("/api/guilds/999/music-search/capabilities");
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.NotNull(rejected.Headers.RetryAfter);
    }

    private sealed class Server(WebApplication app, HttpClient client, Mock<IMusicSearchService> search) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;
        public Mock<IMusicSearchService> Search { get; } = search;

        public static async Task<Server> Create(bool hasAccess)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
            builder.Logging.ClearProviders();
            builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
            builder.Services.AddAuthentication("test").AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("test", _ => { });
            builder.Services.AddAuthorization();
            builder.Services.AddMemoryCache();
            builder.Services.AddMusicSearch(builder.Configuration);
            var users = new Mock<IMobileAppUserRepository>(MockBehavior.Strict);
            users.Setup(u => u.HasGuildAccessAsync(42, It.IsAny<ulong>(), It.IsAny<CancellationToken>())).ReturnsAsync(hasAccess);
            var search = new Mock<IMusicSearchService>(MockBehavior.Strict);
            builder.Services.AddSingleton(users.Object);
            builder.Services.AddSingleton(search.Object);
            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();
            app.MapGroup("/api").MapMusicSearchEndpoints();
            await app.StartAsync();
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            return new Server(app, new HttpClient { BaseAddress = new Uri(address) }, search);
        }

        public async ValueTask DisposeAsync() { Client.Dispose(); await app.DisposeAsync(); }
    }

    private sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(
            Request.Headers.Authorization == "Bearer test"
                ? AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "42") }, "test")), "test"))
                : AuthenticateResult.NoResult());
    }
}
