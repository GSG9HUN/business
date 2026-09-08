namespace API.Requests.Playlists;

public sealed class CreatePlaylistRequest
{
    public string? Name { get; init; }
    public string? PlaylistName { get; init; }

    public string? GetPlaylistName()
    {
        return string.IsNullOrWhiteSpace(PlaylistName) ? Name : PlaylistName;
    }
}
