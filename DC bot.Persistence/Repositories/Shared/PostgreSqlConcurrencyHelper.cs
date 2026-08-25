using System.Data;
using DC_bot.Db;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DC_bot.Repositories.Shared;

internal static class PostgreSqlConcurrencyHelper
{
    private const int DefaultMaxAttempts = 3;

    internal static bool IsUniqueViolation(Exception exception) =>
        FindPostgresException(exception) is { SqlState: PostgresErrorCodes.UniqueViolation };

    internal static bool IsUniqueViolation(DbUpdateException exception) =>
        IsUniqueViolation((Exception)exception);

    internal static bool IsRetriable(Exception exception) =>
        FindPostgresException(exception) is
        {
            SqlState: PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.SerializationFailure
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
        catch (Exception exception) when (IsUniqueViolation(exception))
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
            await using var transaction =
                await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            try
            {
                var result = await operation(dbContext, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch (Exception exception) when (IsRetriable(exception) && attempt < maxAttempts)
            {
                await transaction.RollbackAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(25 * attempt), cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        throw new InvalidOperationException("Operation failed after retries due to concurrent database modifications.");
    }

    internal static async Task ExecuteInSerializableTransactionWithRetryAsync(
        IDbContextFactory<BotDbContext> dbContextFactory,
        Func<BotDbContext, CancellationToken, Task> operation,
        CancellationToken cancellationToken,
        int maxAttempts = DefaultMaxAttempts)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await ExecuteInSerializableTransactionWithRetryAsync(
            dbContextFactory,
            async (dbContext, ct) =>
            {
                await operation(dbContext, ct);
                return true;
            },
            cancellationToken,
            maxAttempts);
    }

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

    private static void DetachEntries(Exception exception)
    {
        if (FindDbUpdateException(exception) is not { } dbUpdateException)
        {
            return;
        }

        foreach (var entry in dbUpdateException.Entries)
        {
            entry.State = EntityState.Detached;
        }
    }

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
