using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DC_bot.Repositories.Shared;

internal static class PostgreSqlConcurrencyHelper
{
    private const int DefaultMaxAttempts = 3;

    internal static bool IsUniqueViolation(Exception exception) =>
        exception switch
        {
            DbUpdateException dbUpdateException => IsUniqueViolation(dbUpdateException),
            PostgresException postgresException => IsUniqueViolation(postgresException),
            _ => false
        };

    internal static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException postgresException &&
        IsUniqueViolation(postgresException);

    internal static bool IsRetriable(Exception exception) =>
        exception switch
        {
            DbUpdateException dbUpdateException => dbUpdateException.InnerException is PostgresException postgresException &&
                                                   IsRetriable(postgresException),
            PostgresException postgresException => IsRetriable(postgresException),
            _ => false
        };

    internal static async Task<bool> SaveChangesIgnoringUniqueViolationAsync(
        DbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            DetachEntries(exception);
            return false;
        }
    }

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
            catch (DbUpdateException exception) when (IsUniqueViolation(exception) && attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(25 * attempt), cancellationToken);
            }
        }
    }

    private static bool IsUniqueViolation(PostgresException exception) =>
        exception.SqlState == PostgresErrorCodes.UniqueViolation;

    private static bool IsRetriable(PostgresException exception) =>
        exception.SqlState is PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.SerializationFailure;

    private static void DetachEntries(DbUpdateException exception)
    {
        foreach (var entry in exception.Entries)
        {
            entry.State = EntityState.Detached;
        }
    }
}
