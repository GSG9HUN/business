namespace API.Requests.Queue;

public sealed class EnqueueRequest(
    string query,
    string? searchMode = null,
    string? voiceChannelId = null,
    string? textChannelId = null)
{
    public string Query { get; init; } = query;
    public string? SearchMode { get; init; } = searchMode;
    public string? VoiceChannelId { get; init; } = voiceChannelId;
    public string? TextChannelId { get; init; } = textChannelId;

    public void Deconstruct(out string query, out string? searchMode)
    {
        query = Query;
        searchMode = SearchMode;
    }
}
