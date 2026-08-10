using System.Text.Json.Serialization;

namespace API.Responses.Auth;

public sealed class DiscordGuildResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("owner")]
    public bool Owner { get; set; }

    [JsonPropertyName("permissions")]
    public string Permissions { get; set; } = string.Empty;
}