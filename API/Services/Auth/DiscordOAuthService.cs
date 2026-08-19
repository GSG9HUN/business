using System.Text.Json;
using API.Services.Auth.Discord.Response;

namespace API.Services.Auth;

public sealed class DiscordOAuthService(HttpClient httpClient, IConfiguration configuration)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string CreateAuthorizeUrl(string state)
    {
        var clientId = configuration["DiscordOAuth:ClientId"]!;
        var redirectUri = Uri.EscapeDataString(configuration["DiscordOAuth:RedirectUri"]!);
        var scope = Uri.EscapeDataString("identify guilds");

        return "https://discord.com/oauth2/authorize" +
               $"?client_id={clientId}" +
               $"&redirect_uri={redirectUri}" +
               "&response_type=code" +
               $"&scope={scope}" +
               $"&state={Uri.EscapeDataString(state)}";
    }

    public async Task<DiscordTokenResponse> ExchangeCodeAsync(string code, CancellationToken ct)
    {
        var body = new Dictionary<string, string>
        {
            ["client_id"] = configuration["DiscordOAuth:ClientId"]!,
            ["client_secret"] = configuration["DiscordOAuth:ClientSecret"]!,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = configuration["DiscordOAuth:RedirectUri"]!
        };

        using var response = await httpClient.PostAsync(
            "https://discord.com/api/oauth2/token",
            new FormUrlEncodedContent(body),
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<DiscordTokenResponse>(json, JsonOptions)!;
    }

    public async Task<DiscordUserResponse> GetCurrentUserAsync(string accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
        request.Headers.Authorization = new("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<DiscordUserResponse>(json, JsonOptions)!;
    }

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
