namespace DC_bot.Exceptions.Messaging;

/// <summary>
///     Exception thrown when Discord message operations fail.
/// </summary>
public class MessageSendException : BotException
{

    /// <summary>
    /// Initializes MessageSendException with the supplied dependencies and settings.
    /// </summary>
    /// <param name="operation">The operation supplied for this operation.</param>
    /// <param name="message">The message supplied for this operation.</param>
    public MessageSendException(string operation, string message) : base($"Message operation '{operation}' failed: {message}")
    {
        Operation = operation;
    }

    /// <summary>
    /// Initializes MessageSendException with the supplied dependencies and settings.
    /// </summary>
    /// <param name="operation">The operation supplied for this operation.</param>
    /// <param name="message">The message supplied for this operation.</param>
    /// <param name="innerException">The inner exception supplied for this operation.</param>
    public MessageSendException(string operation, string message, Exception innerException) : base($"Message operation '{operation}' failed: {message}", innerException)
    {
        Operation = operation;
    }

    /// <summary>
    /// The operation that failed.
    /// </summary>
    public string Operation { get; }
}
