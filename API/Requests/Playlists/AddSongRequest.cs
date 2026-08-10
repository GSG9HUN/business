namespace API.Requests.Playlists;

public sealed class AddSongRequest(string songUrl)
{
    public string SongUrl { get; init; } = songUrl;

    public void Deconstruct(out string songUrl)
    {
        songUrl = SongUrl;
    }
}
