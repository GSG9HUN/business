namespace API.Requests.Playlists;

/// <summary>
/// Carries client input for rename playlist processing; handlers and endpoint filters validate its values.
/// </summary>
/// <param name="newName">The new name value for this operation.</param>
public sealed class RenamePlaylistRequest(string newName)
{

    /// <summary>
    /// The new name requested for the saved playlist.
    /// </summary>
    public string NewName { get; init; } = newName;

    /// <summary>
    /// Exposes the request values through tuple-style deconstruction without validation or normalization.
    /// </summary>
    /// <param name="newName">Receives the new name result.</param>
    public void Deconstruct(out string newName)
    {
        newName = NewName;
    }
}
