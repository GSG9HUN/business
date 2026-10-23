namespace API.Requests.Playlists;

/// <summary>
/// Carries client input for add song processing; handlers and endpoint filters validate its values.
/// </summary>
/// <param name="songUrl">The song url value for this operation.</param>
public sealed class AddSongRequest(string songUrl)
{

    /// <summary>
    /// The URL of the song to add to the playlist.
    /// </summary>
    public string SongUrl { get; init; } = songUrl;

    /// <summary>
    /// Exposes the request values through tuple-style deconstruction without validation or normalization.
    /// </summary>
    /// <param name="songUrl">Receives the song url result.</param>
    public void Deconstruct(out string songUrl)
    {
        songUrl = SongUrl;
    }
}
