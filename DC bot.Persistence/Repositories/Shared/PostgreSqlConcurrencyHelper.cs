using System.Data;
using DC_bot.Db;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DC_bot.Repositories.Shared;

/// <summary>
/// Recognizes supported PostgreSQL conflicts and bounds retries while preserving transaction ownership.
/// </summary>
internal static class PostgreSqlConcurrencyHelper
{
    private const int DefaultMaxAttempts = 3;

    /// <summary>
    /// Checks nested PostgreSQL errors for a unique-constraint violation.
    /// </summary>
    /// <param name = "exception">The original failure inspected or preserved during persistence recovery.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    internal static bool IsUniqueViolation(Exception exception) => FindPostgresException(exception)is { SqlState: PostgresErrorCodes.UniqueViolation };

    /// <summary>
    /// Checks nested PostgreSQL errors for a unique-constraint violation.
    /// </summary>
    /// <param name = "exception">The original failure inspected or preserved during persistence recovery.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    internal static bool IsUniqueViolation(DbUpdateException exception) => IsUniqueViolation((Exception)exception);

    /// <summary>
    /// Checks nested PostgreSQL errors for supported unique or serializable-concurrency conflicts.
    /// </summary>
    /// <param name = "exception">The original failure inspected or preserved during persistence recovery.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    internal static bool IsRetriable(Exception exception) => FindPostgresException(exception)is { SqlState: PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.SerializationFailure };

    /// <summary>
    /// Saves tracked changes, detaching failed entries and returning false only for a recognized unique conflict.
    /// </summary>
    /// <param name = "dbContext">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    internal static async Task<bool> SaveChangesIgnoringUniqueViolationAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception exception)when (IsUniqueViolation(exception))
        {
            DetachEntries(exception);
            return false;
        }
    }

    /// <summary>
    /// Retries the supplied operation for recognized unique conflicts with bounded attempts and cancellable backoff.
    /// </summary>
    /// <param name = "operation">The database callback; transaction retry may invoke it again with a fresh context.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <param name = "maxAttempts">The maximum number of attempts, including the initial one; must be at least one.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    internal static async Task ExecuteWithUniqueViolationRetryAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken,
        int maxAttempts = DefaultMaxAttempts)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (maxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), maxAttempts, "Max attempts must be at least 1.");
        }

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await operation(cancellationToken);
                return;
            }
            catch (DbUpdateException exception)when (IsUniqueViolation(exception) && attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(25 * attempt), cancellationToken);
            }
        }
    }

    /// <summary>
    /// Runs the database callback in a fresh serializable transaction per attempt, retrying recognized conflicts only after successful owned rollback.
    /// </summary>
    /// <remarks>The callback may run more than once with a fresh context. Keep external network or playback side effects outside this callback. Rollback uses an independent deadline; failed rollback prevents another attempt and remains attached to the original exception.</remarks>
    /// <param name = "dbContextFactory">The injected factory used to create a fresh context for each operation or transaction attempt.</param>
    /// <param name = "operation">The database callback; transaction retry may invoke it again with a fresh context.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <param name = "maxAttempts">The maximum number of attempts, including the initial one; must be at least one.</param>
    /// <typeparam name = "T">The value produced by the database callback or mapped property.</typeparam>
    /// <returns>A task yielding the persisted or mapped result described above.</returns>
    internal static async Task<T> ExecuteInSerializableTransactionWithRetryAsync<T>(
        IDbContextFactory<BotDbContext> dbContextFactory,
        Func<BotDbContext, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken,
        int maxAttempts = DefaultMaxAttempts)
    {
        ArgumentNullException.ThrowIfNull(dbContextFactory);
        ArgumentNullException.ThrowIfNull(operation);

        if (maxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), maxAttempts, "Max attempts must be at least 1.");
        }

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var result = await operation(dbContext, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch (Exception exception)when (IsRetriable(exception) && attempt < maxAttempts)
            {
                if (!await TransactionCleanup.TryRollbackAsync(transaction, exception))
                {
                    throw;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(25 * attempt), cancellationToken);
            }
            catch (Exception operationException)
            {
                await TransactionCleanup.TryRollbackAsync(transaction, operationException);
                throw;
            }
        }

        throw new InvalidOperationException("Operation failed after retries due to concurrent database modifications.");
    }

    /// <summary>
    /// Runs the database callback in a fresh serializable transaction per attempt, retrying recognized conflicts only after successful owned rollback.
    /// </summary>
    /// <remarks>The callback may run more than once with a fresh context. Keep external network or playback side effects outside this callback. Rollback uses an independent deadline; failed rollback prevents another attempt and remains attached to the original exception.</remarks>
    /// <param name = "dbContextFactory">The injected factory used to create a fresh context for each operation or transaction attempt.</param>
    /// <param name = "operation">The database callback; transaction retry may invoke it again with a fresh context.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <param name = "maxAttempts">The maximum number of attempts, including the initial one; must be at least one.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    internal static async Task ExecuteInSerializableTransactionWithRetryAsync(
        IDbContextFactory<BotDbContext> dbContextFactory,
        Func<BotDbContext, CancellationToken, Task> operation,
        CancellationToken cancellationToken,
        int maxAttempts = DefaultMaxAttempts)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await ExecuteInSerializableTransactionWithRetryAsync(dbContextFactory, async (dbContext, ct) =>
        {
            await operation(dbContext, ct);
            return true;
        }, cancellationToken, maxAttempts);
    }

    /// <summary>
    /// Finds a nested PostgreSQL exception without changing the original exception chain.
    /// </summary>
    /// <param name = "exception">The original failure inspected or preserved during persistence recovery.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    private static PostgresException? FindPostgresException(Exception exception)
    {
        var current = exception;

        while (current is not null)
        {
            if (current is PostgresException postgresException)
            {
                return postgresException;
            }

            current = current.InnerException;
        }

        return null;
    }

    /// <summary>
    /// Detaches entries from a failed EF update so a handled unique conflict does not remain pending.
    /// </summary>
    /// <param name = "exception">The original failure inspected or preserved during persistence recovery.</param>
    private static void DetachEntries(Exception exception)
    {
        if (FindDbUpdateException(exception)is not { } dbUpdateException)
        {
            return;
        }

        foreach (var entry in dbUpdateException.Entries)
        {
            entry.State = EntityState.Detached;
        }
    }

    /// <summary>
    /// Finds a nested EF update exception whose tracked entries require detaching.
    /// </summary>
    /// <param name = "exception">The original failure inspected or preserved during persistence recovery.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    private static DbUpdateException? FindDbUpdateException(Exception exception)
    {
        var current = exception;

        while (current is not null)
        {
            if (current is DbUpdateException dbUpdateException)
            {
                return dbUpdateException;
            }

            current = current.InnerException;
        }

        return null;
    }
}
