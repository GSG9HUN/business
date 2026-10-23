using System.Text.Json;
using API.Services.Auth.Discord.Response;
using Microsoft.Extensions.Options;

namespace API.Services.Auth;

/// <summary>
/// Uses a factory-managed HTTP client to exchange Discord OAuth codes and read user and guild data.
/// </summary>
/// <param name="httpClient">The HTTP client managed by the dependency-injection HTTP client factory.</param>
/// <param name="options">The injected typed configuration for this component.</param>
public sealed class DiscordOAuthService(HttpClient httpClient, IOptions<DiscordOAuthOptions> options)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Builds the Discord OAuth authorization URL with identify and guilds scopes and the supplied state.
    /// </summary>
    /// <param name="state">The state value for this operation.</param>
    /// <returns>The computed create authorize url result.</returns>
    public string CreateAuthorizeUrl(string state)
    {
        var clientId = options.Value.ClientId;
        var redirectUri = Uri.EscapeDataString(options.Value.RedirectUri);
        var scope = Uri.EscapeDataString("identify guilds");
        return "https://discord.com/oauth2/authorize" + $"?client_id={clientId}" + $"&redirect_uri={redirectUri}" + "&response_type=code" + $"&scope={scope}" + $"&state={Uri.EscapeDataString(state)}";
    }

    /// <summary>
    /// Exchanges a Discord authorization code for OAuth tokens using the configured redirect URI.
    /// </summary>
    /// <param name="code">The code value for this operation.</param>
    /// <param name="ct">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The computed exchange code result.</returns>
    public async Task<DiscordTokenResponse> ExchangeCodeAsync(string code, CancellationToken ct)
    {
        var body = new Dictionary<string, string>
        {
            ["client_id"] = options.Value.ClientId,
            ["client_secret"] = options.Value.ClientSecret,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = options.Value.RedirectUri
        };
        using var content = new FormUrlEncodedContent(body);
        using var response = await httpClient.PostAsync("https://discord.com/api/oauth2/token", content, ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<DiscordTokenResponse>(json, JsonOptions)!;
    }

    /// <summary>
    /// Reads the Discord user represented by the OAuth bearer token.
    /// </summary>
    /// <param name="accessToken">The access token value for this operation.</param>
    /// <param name="ct">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The computed get current user result.</returns>
    public async Task<DiscordUserResponse> GetCurrentUserAsync(string accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
        request.Headers.Authorization = new("Bearer", accessToken);
        using var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<DiscordUserResponse>(json, JsonOptions)!;
    }

    /// <summary>
    /// Reads the guild memberships visible to the Discord OAuth bearer token.
    /// </summary>
    /// <param name="accessToken">The access token value for this operation.</param>
    /// <param name="ct">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The computed get current user guilds result.</returns>
    public async Task<IReadOnlyList<DiscordGuildResponse>> GetCurrentUserGuildsAsync(string accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me/guilds");
        request.Headers.Authorization = new("Bearer", accessToken);
        using var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<IReadOnlyList<DiscordGuildResponse>>(json, JsonOptions)!;
    }
}
