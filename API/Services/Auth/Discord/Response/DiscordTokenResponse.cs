using System.Text.Json.Serialization;

namespace API.Services.Auth.Discord.Response;

/// <summary>
/// Deserializes Discord OAuth token fields returned by the Discord REST API.
/// </summary>
public sealed class DiscordTokenResponse
{

    /// <summary>
    /// The access token returned by Discord OAuth.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// The authorization scheme returned with the access token.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// The access token lifetime reported by Discord, in seconds.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// The refresh token returned by Discord, or null when absent.
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// The OAuth scopes granted by Discord.
    /// </summary>
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}
