namespace DC_bot.Exceptions;

/// <summary>
///     Base exception for all bot-related errors.
/// </summary>
public abstract class BotException : Exception
{
    protected BotException(string message) : base(message)
    {
    }

    protected BotException(string message, Exception innerException) : base(message, innerException)
    {
    }
}