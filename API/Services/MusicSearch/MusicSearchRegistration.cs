using System.Threading.RateLimiting;
using API.Responses.MusicSearch;
using Microsoft.Extensions.Options;

namespace API.Services.MusicSearch;

public static class MusicSearchRegistration
{
    public static IServiceCollection AddMusicSearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MusicSearchOptions>().Bind(configuration.GetSection("MusicSearch"))
            .Configure(options =>
            {
                var host = configuration["LAVALINK_HOSTNAME"];
                if (!string.IsNullOrWhiteSpace(host))
                {
                    var secured = string.Equals(configuration["LAVALINK_SECURED"], "true", StringComparison.OrdinalIgnoreCase);
                    var port = int.TryParse(configuration["LAVALINK_PORT"], out var parsed) ? parsed : 2333;
                    options.BaseAddress = new UriBuilder(secured ? "https" : "http", host, port).Uri.AbsoluteUri;
                }
                options.Password = configuration["LAVALINK_PASSWORD"] ?? options.Password;
            })
            .Validate(o => o.TimeoutSeconds is >= 1 and <= 30 && o.MaxResults is >= 1 and <= 20,
                "Music search timeout must be 1-30 seconds and MaxResults 1-20.")
            .Validate(o => Uri.TryCreate(o.BaseAddress, UriKind.Absolute, out var uri) &&
                           uri.Scheme is "http" or "https" && string.IsNullOrEmpty(uri.UserInfo),
                "Music search requires an HTTP(S) Lavalink base address.")
            .ValidateOnStart();
        services.AddHttpClient<LavalinkSearchClient>((provider, client) =>
            {
                client.BaseAddress = new Uri(provider.GetRequiredService<IOptions<MusicSearchOptions>>().Value.BaseAddress.TrimEnd('/') + "/");
                client.Timeout = Timeout.InfiniteTimeSpan; 
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                MaxConnectionsPerServer = 8
            })
            .RemoveAllLoggers(); 
        services.AddTransient<IMusicSearchService, MusicSearchService>();
        services.AddSingleton<MusicSearchQuota>();
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("music-search", context => RateLimitPartition.GetFixedWindowLimiter(
                context.User.FindFirst("sub")?.Value ?? "anonymous",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 60, Window = TimeSpan.FromMinutes(1), QueueLimit = 0
                }));
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.Headers.RetryAfter = "60";
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new MusicSearchErrorResponse("SearchRateLimited", "Too many search requests. Retry later."), token);
            };
        });
        return services;
    }
}
