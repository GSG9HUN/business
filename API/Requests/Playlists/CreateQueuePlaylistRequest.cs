namespace API.Requests.Playlists;

/// <summary>Captures ordered queue occurrences and their source URLs at the user's save action.</summary>
public sealed class CreateQueuePlaylistRequest
{

    /// <summary>Gets the name of the new saved playlist.</summary>
    public string? PlaylistName { get; init; }

    /// <summary>Gets source URLs in history, current, then waiting order, retaining duplicates.</summary>
    public IReadOnlyList<string>? TrackUrls { get; init; }

    /// <summary>Gets stable queue occurrence IDs matching the URL list one-to-one.</summary>
    public IReadOnlyList<string>? QueueItemIds { get; init; }
}
