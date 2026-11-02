using DC_bot.BotControl;
using DC_bot.Db;
using DC_bot.Entities.BotControl;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.BotControl;

/// <summary>
/// Reads and mutates bot control commands state using short-lived contexts from the injected EF factory.
/// </summary>
/// <param name = "dbContextFactory">The injected factory used to create a fresh context for each operation or transaction attempt.</param>
public class BotControlCommandsRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IBotControlCommandsRepository
{
    private const string WorkerWakeUpChannel = "bot_control_commands";
    private const string MobileRealtimeCommandUpdatesChannel = "bot_control_command_updates";

    /// <inheritdoc/>
    public async Task<BotControlCommandRecord?> GetByCommandIdAsync(string commandId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(commandId))
        {
            return null;
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.BotControlCommands.AsNoTracking()
            .FirstOrDefaultAsync(command => command.CommandId == commandId, cancellationToken);
        return entity is null ? null : MapToRecord(entity);
    }

    /// <inheritdoc/>
    public async Task<BotControlCommandRecord> EnqueueAsync(
        ulong guildId,
        ulong userId,
        string type,
        CancellationToken cancellationToken)
    {
        return await EnqueueAsync(guildId, userId, type, null, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<BotControlCommandRecord> EnqueueAsync(
        ulong guildId,
        ulong userId,
        string type,
        string? payloadJson,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var botCommand = new BotControlCommandEntity
            {
                CommandId = Guid.NewGuid()
                    .ToString(),
                GuildId = guildId,
                Type = type,
                PayloadJson = payloadJson,
                UserId = userId,
                Status = BotControlCommandState.Pending,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            dbContext.BotControlCommands.Add(botCommand);
            await dbContext.SaveChangesAsync(cancellationToken);
            await NotifyWorkerAsync(dbContext, cancellationToken);
            await NotifyCommandUpdatedAsync(dbContext, botCommand.CommandId, MobileRealtimeEventNames.BotControlCommandCreated, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return MapToRecord(botCommand);
        }
        catch (Exception operationException)
        {
            await TransactionCleanup.TryRollbackAsync(transaction, operationException);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<BotControlCommandRecord?> ClaimNextPendingAsync(CancellationToken cancellationToken)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(dbContextFactory, ClaimNextPendingAsync, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BotControlCommandRecord>> GetStartedAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var commands = await dbContext.BotControlCommands.AsNoTracking()
            .Where(command => command.Status == BotControlCommandState.Started)
            .OrderBy(command => command.ClaimedAtUtc ?? command.CreatedAtUtc)
            .ToListAsync(cancellationToken);
        return commands.Select(MapToRecord)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task MarkDoneAsync(string commandId, string? resultJson, CancellationToken cancellationToken)
    {
        await UpdateStatusAsync(commandId, BotControlCommandState.Done, null, resultJson, MobileRealtimeEventNames.BotControlCommandSucceeded, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task MarkFailedAsync(string commandId, string errorMessage, string? resultJson, CancellationToken cancellationToken)
    {
        await UpdateStatusAsync(commandId, BotControlCommandState.Failed, errorMessage, resultJson, MobileRealtimeEventNames.BotControlCommandFailed, cancellationToken);
    }

    /// <summary>
    /// Atomically claims the oldest eligible pending command so competing workers cannot execute the same claim.
    /// </summary>
    /// <param name = "dbContext">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    private static async Task<BotControlCommandRecord?> ClaimNextPendingAsync(
        BotDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var command = await dbContext.BotControlCommands.Where(x => x.Status == BotControlCommandState.Pending)
            .OrderBy(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (command is null)
        {
            return null;
        }

        command.Status = BotControlCommandState.Started;
        command.ClaimedAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await NotifyCommandUpdatedAsync(dbContext, command.CommandId, MobileRealtimeEventNames.BotControlCommandStarted, cancellationToken);
        return MapToRecord(command);
    }

    /// <summary>
    /// Updates one command&apos;s durable terminal state and notification payload inside a transaction.
    /// </summary>
    /// <param name = "commandId">The command id value used by this storage contract.</param>
    /// <param name = "status">The status value used by this storage contract.</param>
    /// <param name = "errorMessage">The error message value used by this storage contract.</param>
    /// <param name = "resultJson">The result json value used by this storage contract.</param>
    /// <param name = "eventName">The shared realtime event name, or the documented default when omitted.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    private async Task UpdateStatusAsync(
        string commandId,
        BotControlCommandState status,
        string? errorMessage,
        string? resultJson,
        string eventName,
        CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var command = await db.BotControlCommands.FirstAsync(x => x.CommandId == commandId, cancellationToken);
            command.Status = status;
            command.ErrorMessage = errorMessage;
            command.ResultJson = resultJson;
            command.CompletedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            await NotifyCommandUpdatedAsync(db, command.CommandId, eventName, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception operationException)
        {
            await TransactionCleanup.TryRollbackAsync(transaction, operationException);
            throw;
        }
    }

    /// <summary>
    /// Issues the PostgreSQL notification that wakes the durable command worker after commit.
    /// </summary>
    /// <remarks>The notification is part of the current PostgreSQL transaction and becomes observable on commit. This method does not commit the caller&apos;s transaction.</remarks>
    /// <param name = "dbContext">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    private static Task NotifyWorkerAsync(BotDbContext dbContext, CancellationToken cancellationToken)
    {
        return NotifyAsync(dbContext, WorkerWakeUpChannel, null, cancellationToken);
    }

    /// <summary>
    /// Publishes the command identifier and event name on the realtime command channel.
    /// </summary>
    /// <remarks>The notification is part of the current PostgreSQL transaction and becomes observable on commit. This method does not commit the caller&apos;s transaction.</remarks>
    /// <param name = "dbContext">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "commandId">The command id value used by this storage contract.</param>
    /// <param name = "eventName">The shared realtime event name, or the documented default when omitted.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    private static Task NotifyCommandUpdatedAsync(
        BotDbContext dbContext,
        string commandId,
        string eventName,
        CancellationToken cancellationToken)
    {
        var payload = $"{commandId}|{eventName}";
        return NotifyAsync(dbContext, MobileRealtimeCommandUpdatesChannel, payload, cancellationToken);
    }

    /// <summary>
    /// Publishes a parameterized PostgreSQL notification using the caller&apos;s current context and transaction.
    /// </summary>
    /// <remarks>The notification is part of the current PostgreSQL transaction and becomes observable on commit. This method does not commit the caller&apos;s transaction.</remarks>
    /// <param name = "dbContext">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "channel">The channel value used by this storage contract.</param>
    /// <param name = "payload">The payload value used by this storage contract.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    private static Task NotifyAsync(BotDbContext dbContext, string channel, string? payload, CancellationToken cancellationToken)
    {
        if (!dbContext.Database.IsRelational())
        {
            return Task.CompletedTask;
        }

        return payload is null ? dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_notify({channel}, '');", cancellationToken) : dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_notify({channel}, {payload});", cancellationToken);
    }

    /// <summary>
    /// Copies persisted entity values into an immutable storage contract without exposing EF tracking.
    /// </summary>
    /// <param name = "entity">The entity value used by this storage contract.</param>
    /// <returns>The mapped, normalized, or configured value described above.</returns>
    private static BotControlCommandRecord MapToRecord(BotControlCommandEntity entity)
    {
        return new BotControlCommandRecord(entity.CommandId, entity.GuildId, entity.UserId, entity.Type, entity.Status, entity.PayloadJson, entity.ErrorMessage, entity.CreatedAtUtc, entity.ClaimedAtUtc, entity.CompletedAtUtc, entity.ResultJson);
    }
}
