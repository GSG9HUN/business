using Microsoft.EntityFrameworkCore.Storage;

namespace DC_bot.Repositories.Shared;

/// <summary>
/// Rolls back a failed operation with a fresh deadline while preserving its original exception.
/// </summary>
internal static class TransactionCleanup
{
    internal const string RollbackFailureKey = "DC_bot.TransactionRollbackFailure";
    private static readonly TimeSpan RollbackDeadline = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Attempts owned rollback independently of caller cancellation and records cleanup failure on the original exception.
    /// </summary>
    /// <param name = "transaction">The transaction whose failed operation is being unwound.</param>
    /// <param name = "operationException">The original exception that must remain the primary failure.</param>
    /// <returns>True when rollback completed; false when cleanup failed and retry must not proceed.</returns>
    internal static async Task<bool> TryRollbackAsync(IDbContextTransaction transaction, Exception operationException)
    {
        using var deadline = new CancellationTokenSource(RollbackDeadline);
        try
        {
            await transaction.RollbackAsync(deadline.Token)
                .ConfigureAwait(false);
            return true;
        }
        catch (Exception rollbackException)
        {
            operationException.Data[RollbackFailureKey] = rollbackException;
            return false;
        }
    }
}
