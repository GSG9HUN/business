namespace DC_bot.Exceptions;

/// <summary>
/// Exception thrown when music queue operations fail.
/// </summary>
public class QueueOperationException : BotException
{
    public string Operation { get; }
    public ulong GuildId { get; }

    public QueueOperationException(string operation, ulong guildId, string message) 
        : base($"Queue operation '{operation}' failed for guild {guildId}: {message}")
    {
        Operation = operation;
        GuildId = guildId;
    }

    public QueueOperationException(string operation, ulong guildId, string message, Exception innerException) 
        : base($"Queue operation '{operation}' failed for guild {guildId}: {message}", innerException)
    {
        Operation = operation;
        GuildId = guildId;
    }
}

