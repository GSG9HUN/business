namespace DC_bot.Exceptions.Music;

/// <summary>
///     Exception thrown when a track cannot be loaded or played.
/// </summary>
public class TrackLoadException : BotException
{

    /// <summary>
    /// Initializes TrackLoadException with the supplied dependencies and settings.
    /// </summary>
    /// <param name="query">The query supplied for this operation.</param>
    /// <param name="message">The message supplied for this operation.</param>
    public TrackLoadException(string query, string message) : base($"Failed to load track '{query}': {message}")
    {
        Query = query;
    }

    /// <summary>
    /// Initializes TrackLoadException with the supplied dependencies and settings.
    /// </summary>
    /// <param name="query">The query supplied for this operation.</param>
    /// <param name="message">The message supplied for this operation.</param>
    /// <param name="innerException">The inner exception supplied for this operation.</param>
    public TrackLoadException(string query, string message, Exception innerException) : base($"Failed to load track '{query}': {message}", innerException)
    {
        Query = query;
    }

    /// <summary>
    /// The search query or track URL.
    /// </summary>
    public string Query { get; }
}
