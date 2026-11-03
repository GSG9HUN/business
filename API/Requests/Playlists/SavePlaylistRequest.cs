namespace API.Requests.Playlists;

/// <summary>
/// Carries client input for save playlist processing; handlers and endpoint filters validate its values.
/// </summary>
/// <param name="name">The name value for this operation.</param>
/// <param name="url">The url value for this operation.</param>
public sealed class SavePlaylistRequest(string name, string url)
{

    /// <summary>
    /// The saved playlist name.
    /// </summary>
    public string Name { get; init; } = name;

    /// <summary>
    /// The URL from which playlist tracks should be imported.
    /// </summary>
    public string Url { get; init; } = url;

    /// <summary>
    /// Exposes the request values through tuple-style deconstruction without validation or normalization.
    /// </summary>
    /// <param name="name">Receives the name result.</param>
    /// <param name="url">Receives the url result.</param>
    public void Deconstruct(out string name, out string url)
    {
        name = Name;
        url = Url;
    }
}
