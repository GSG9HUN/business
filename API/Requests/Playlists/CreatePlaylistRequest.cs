namespace API.Requests.Playlists;

/// <summary>
/// Carries client input for create playlist processing; handlers and endpoint filters validate its values.
/// </summary>
public sealed class CreatePlaylistRequest
{

    /// <summary>
    /// The legacy playlist name used when PlaylistName is absent or blank.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The explicit playlist name, preferred over the legacy Name field.
    /// </summary>
    public string? PlaylistName { get; init; }

    /// <summary>
    /// Returns the explicitly supplied playlist name, falling back to the legacy name field.
    /// </summary>
    /// <returns>The converted value, or null when the input is absent or cannot be represented.</returns>
    public string? GetPlaylistName()
    {
        return string.IsNullOrWhiteSpace(PlaylistName) ? Name : PlaylistName;
    }
}
