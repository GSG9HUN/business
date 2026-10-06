namespace API.Services.MusicSearch;

public sealed class MusicSearchOptions
{
    public string BaseAddress { get; set; } = "http://localhost:2333/";
    public string Password { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 10;
    public int MaxResults { get; set; } = 20;
    public Dictionary<string, MusicSearchProviderOptions> Providers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class MusicSearchProviderOptions
{
    // Explicit deployment rollout switches, independent of installed source managers.
    public bool Enabled { get; set; }
    public bool PlaylistSearch { get; set; }
    public bool CanEnqueue { get; set; }
    public string? UnavailableReason { get; set; }
}
