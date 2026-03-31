namespace DC_bot.Exceptions.Music;

/// <summary>
///     Exception thrown when a track cannot be loaded or played.
/// </summary>
public class TrackLoadException : BotException
{
    public TrackLoadException(string query, string message)
        : base($"Failed to load track '{query}': {message}")
    {
        Query = query;
    }

    public TrackLoadException(string query, string message, Exception innerException)
        : base($"Failed to load track '{query}': {message}", innerException)
    {
        Query = query;
    }

    public string Query { get; }
}