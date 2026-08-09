using System.Text.Json.Serialization;

namespace API.Responses.Auth;

public sealed class DiscordGuildResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("owner")]
    public bool Owner { get; set; }

    [JsonPropertyName("permissions")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public ulong Permissions { get; set; }
    
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }
}