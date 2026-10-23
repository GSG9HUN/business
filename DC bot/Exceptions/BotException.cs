namespace DC_bot.Exceptions;

/// <summary>
///     Base exception for all bot-related errors.
/// </summary>
public abstract class BotException : Exception
{

    /// <summary>
    /// Initializes BotException with the supplied dependencies and settings.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    protected BotException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes BotException with the supplied dependencies and settings.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <param name="innerException">The inner exception supplied for this operation.</param>
    protected BotException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
