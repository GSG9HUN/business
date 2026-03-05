namespace DC_bot.Exceptions.Messaging;

/// <summary>
/// Exception thrown when Discord message operations fail.
/// </summary>
public class MessageSendException : BotException
{
    public string Operation { get; }

    public MessageSendException(string operation, string message) 
        : base($"Message operation '{operation}' failed: {message}")
    {
        Operation = operation;
    }

    public MessageSendException(string operation, string message, Exception innerException) 
        : base($"Message operation '{operation}' failed: {message}", innerException)
    {
        Operation = operation;
    }
}

