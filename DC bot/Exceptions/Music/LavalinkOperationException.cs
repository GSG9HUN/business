namespace DC_bot.Exceptions.Music;

/// <summary>
/// Exception thrown when Lavalink operations fail.
/// </summary>
public class LavalinkOperationException : BotException
{
    public string Operation { get; }

    public LavalinkOperationException(string operation, string message) 
        : base($"Lavalink operation '{operation}' failed: {message}")
    {
        Operation = operation;
    }

    public LavalinkOperationException(string operation, string message, Exception innerException) 
        : base($"Lavalink operation '{operation}' failed: {message}", innerException)
    {
        Operation = operation;
    }
}

