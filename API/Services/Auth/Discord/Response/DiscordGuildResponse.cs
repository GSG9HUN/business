using System.Text.Json.Serialization;

namespace API.Services.Auth.Discord.Response;

/// <summary>
/// Deserializes Discord OAuth guild fields returned by the Discord REST API.
/// </summary>
public sealed class DiscordGuildResponse
{

    /// <summary>
    /// The Discord guild identifier serialized as a string.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the authenticated Discord user owns the guild.
    /// </summary>
    [JsonPropertyName("owner")]
    public bool Owner { get; set; }

    /// <summary>
    /// The Discord permission bit field for the user's guild membership.
    /// </summary>
    [JsonPropertyName("permissions")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public ulong Permissions { get; set; }

    /// <summary>
    /// The Discord guild icon hash, or null when no icon is set.
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }
}
