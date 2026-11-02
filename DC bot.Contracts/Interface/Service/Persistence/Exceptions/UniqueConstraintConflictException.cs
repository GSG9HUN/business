namespace DC_bot.Interface.Service.Persistence.Exceptions;

/// <summary>
/// Reports a translated unique-storage conflict while retaining the original persistence failure.
/// </summary>
public sealed class UniqueConstraintConflictException : Exception
{

    /// <summary>
    /// Initializes UniqueConstraintConflictException using the supplied persistence dependencies without constructing fallback services.
    /// </summary>
    /// <param name = "message">The message value used by this storage contract.</param>
    /// <param name = "innerException">The inner exception value used by this storage contract.</param>
    public UniqueConstraintConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
