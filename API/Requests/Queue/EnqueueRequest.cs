namespace API.Requests.Queue;

/// <summary>
/// Carries client input for enqueue processing; handlers and endpoint filters validate its values.
/// </summary>
/// <param name="query">The query value for this operation.</param>
/// <param name="searchMode">The search mode value for this operation.</param>
/// <param name="voiceChannelId">The voice channel id value for this operation.</param>
/// <param name="textChannelId">The text channel id value for this operation.</param>
public sealed class EnqueueRequest(
    string query,
    string? searchMode = null,
    string? voiceChannelId = null,
    string? textChannelId = null)
{

    /// <summary>
    /// The search query or track URL.
    /// </summary>
    public string Query { get; init; } = query;

    /// <summary>
    /// The optional search provider or mode used to resolve the query.
    /// </summary>
    public string? SearchMode { get; init; } = searchMode;

    /// <summary>
    /// The requested Discord voice channel identifier, or null when unspecified.
    /// </summary>
    public string? VoiceChannelId { get; init; } = voiceChannelId;

    /// <summary>
    /// The requested Discord text channel identifier, or null when unspecified.
    /// </summary>
    public string? TextChannelId { get; init; } = textChannelId;

    /// <summary>
    /// Exposes the request values through tuple-style deconstruction without validation or normalization.
    /// </summary>
    /// <param name="query">Receives the query result.</param>
    /// <param name="searchMode">Receives the search mode result.</param>
    public void Deconstruct(out string query, out string? searchMode)
    {
        query = Query;
        searchMode = SearchMode;
    }
}
