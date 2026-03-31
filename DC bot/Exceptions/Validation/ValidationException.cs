namespace DC_bot.Exceptions.Validation;

/// <summary>
///     Exception thrown when validation fails.
/// </summary>
public class ValidationException : BotException
{
    public ValidationException(string validationKey, string message)
        : base(message)
    {
        ValidationKey = validationKey;
    }

    public ValidationException(string validationKey, string message, Exception innerException)
        : base(message, innerException)
    {
        ValidationKey = validationKey;
    }

    public string ValidationKey { get; }
}