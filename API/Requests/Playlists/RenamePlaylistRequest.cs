namespace API.Requests.Playlists;

public sealed class RenamePlaylistRequest(string newName)
{
    public string NewName { get; init; } = newName;

    public void Deconstruct(out string newName)
    {
        newName = NewName;
    }
}
