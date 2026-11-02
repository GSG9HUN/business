namespace API.Services.MusicSearch;

/// <summary>
/// Controls search and enqueue availability for an individual deployed music provider.
/// </summary>
public sealed class MusicSearchProviderOptions
{
    // Explicit deployment rollout switches, independent of installed source managers.

    /// <summary>
    /// Whether this search provider is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Whether this provider supports playlist searches.
    /// </summary>
    public bool PlaylistSearch { get; set; }

    /// <summary>
    /// Whether tracks from this provider may be added to the queue.
    /// </summary>
    public bool CanEnqueue { get; set; }

    /// <summary>
    /// The explanation shown when the provider is unavailable, or null when available.
    /// </summary>
    public string? UnavailableReason { get; set; }
}
