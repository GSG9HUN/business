namespace API.Services.MusicSearch;

/// <summary>
/// Configures the Lavalink search endpoint, timeout, result limit, and provider rollout settings.
/// </summary>
public sealed class MusicSearchOptions
{

    /// <summary>
    /// The base address of the Lavalink server used for music searches.
    /// </summary>
    public string BaseAddress { get; set; } = "http://localhost:2333/";

    /// <summary>
    /// The password used to authenticate requests to Lavalink.
    /// </summary>
    public string Password { get; set; } = "";

    /// <summary>
    /// The maximum duration of a music search request, in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// The maximum number of search results returned to the client.
    /// </summary>
    public int MaxResults { get; set; } = 20;

    /// <summary>
    /// The search capabilities configured for each supported provider.
    /// </summary>
    public Dictionary<string, MusicSearchProviderOptions> Providers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
