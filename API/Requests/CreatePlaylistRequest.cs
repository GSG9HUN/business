namespace API.Requests;

public sealed class CreatePlaylistRequest(string name)
{
    public string Name { get; init; } = name;

    public void Deconstruct(out string name)
    {
        name = Name;
    }
}
