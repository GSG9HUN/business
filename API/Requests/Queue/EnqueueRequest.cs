namespace API.Requests.Queue;

public sealed class EnqueueRequest(string query, string? searchMode = null)
{
    public string Query { get; init; } = query;
    public string? SearchMode { get; init; } = searchMode;

    public void Deconstruct(out string query, out string? searchMode)
    {
        query = Query;
        searchMode = SearchMode;
    }
}