namespace DC_bot.Exceptions.Music;

/// <summary>
///     Exception thrown when Lavalink operations fail.
/// </summary>
public class LavalinkOperationException : BotException
{

    /// <summary>
    /// Initializes LavalinkOperationException with the supplied dependencies and settings.
    /// </summary>
    /// <param name="operation">The operation supplied for this operation.</param>
    /// <param name="message">The message supplied for this operation.</param>
    public LavalinkOperationException(string operation, string message) : base($"Lavalink operation '{operation}' failed: {message}")
    {
        Operation = operation;
    }

    /// <summary>
    /// Initializes LavalinkOperationException with the supplied dependencies and settings.
    /// </summary>
    /// <param name="operation">The operation supplied for this operation.</param>
    /// <param name="message">The message supplied for this operation.</param>
    /// <param name="innerException">The inner exception supplied for this operation.</param>
    public LavalinkOperationException(string operation, string message, Exception innerException) : base($"Lavalink operation '{operation}' failed: {message}", innerException)
    {
        Operation = operation;
    }

    /// <summary>
    /// The operation that failed.
    /// </summary>
    public string Operation { get; }
}
