using System.Text.Json.Serialization;

namespace API.Services.Auth.Discord.Response;

/// <summary>
/// Deserializes Discord OAuth user fields returned by the Discord REST API.
/// </summary>
public sealed class DiscordUserResponse
{

    /// <summary>
    /// The Discord user identifier serialized as a string.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The Discord username.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The Discord global display name, or null when it is not set.
    /// </summary>
    [JsonPropertyName("global_name")]
    public string? GlobalName { get; set; }

    /// <summary>
    /// The Discord avatar hash, or null when no custom avatar is set.
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }
}
