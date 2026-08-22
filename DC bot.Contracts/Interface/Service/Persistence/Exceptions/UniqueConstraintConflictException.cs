namespace DC_bot.Interface.Service.Persistence.Exceptions;

public sealed class UniqueConstraintConflictException : Exception
{
    public UniqueConstraintConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
