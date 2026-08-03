namespace API.Requests;

public sealed class SavePlaylistRequest(string name, string url)
{
    public string Name { get; init; } = name;
    public string Url { get; init; } = url;

    public void Deconstruct(out string name, out string url)
    {
        name = Name;
        url = Url;
    }
}
