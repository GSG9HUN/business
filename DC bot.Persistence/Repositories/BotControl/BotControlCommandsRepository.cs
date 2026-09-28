using DC_bot.BotControl;
using DC_bot.Db;
using DC_bot.Entities.BotControl;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.BotControl;

public class BotControlCommandsRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IBotControlCommandsRepository
{
    private const string WorkerWakeUpChannel = "bot_control_commands";
    private const string MobileRealtimeCommandUpdatesChannel = "bot_control_command_updates";

    public async Task<BotControlCommandRecord?> GetByCommandIdAsync(
        string commandId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(commandId))
        {
            return null;
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.BotControlCommands
            .AsNoTracking()
            .FirstOrDefaultAsync(command => command.CommandId == commandId, cancellationToken);

        return entity is null ? null : MapToRecord(entity);
    }

    public async Task<BotControlCommandRecord> EnqueueAsync(
        ulong guildId,
        ulong userId,
        string type,
        CancellationToken cancellationToken)
    {
        return await EnqueueAsync(guildId, userId, type, null, cancellationToken);
    }

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
                CommandId = Guid.NewGuid().ToString(),
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
            await NotifyCommandUpdatedAsync(
                dbContext,
                botCommand.CommandId,
                MobileRealtimeEventNames.BotControlCommandCreated,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return MapToRecord(botCommand);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public Task<BotControlCommandRecord?> ClaimNextPendingAsync(CancellationToken cancellationToken)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            dbContextFactory,
            ClaimNextPendingAsync,
            cancellationToken);
    }

    public async Task<IReadOnlyList<BotControlCommandRecord>> GetStartedAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var commands = await dbContext.BotControlCommands
            .AsNoTracking()
            .Where(command => command.Status == BotControlCommandState.Started)
            .OrderBy(command => command.ClaimedAtUtc ?? command.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return commands.Select(MapToRecord).ToList();
    }

    public async Task MarkDoneAsync(string commandId, string? resultJson, CancellationToken cancellationToken)
    {
        await UpdateStatusAsync(
            commandId,
            BotControlCommandState.Done,
            null,
            resultJson,
            MobileRealtimeEventNames.BotControlCommandSucceeded,
            cancellationToken);
    }

    public async Task MarkFailedAsync(
        string commandId,
        string errorMessage,
        string? resultJson,
        CancellationToken cancellationToken)
    {
        await UpdateStatusAsync(
            commandId,
            BotControlCommandState.Failed,
            errorMessage,
            resultJson,
            MobileRealtimeEventNames.BotControlCommandFailed,
            cancellationToken);
    }

    private static async Task<BotControlCommandRecord?> ClaimNextPendingAsync(
        BotDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var command = await dbContext.BotControlCommands
            .Where(x => x.Status == BotControlCommandState.Pending)
            .OrderBy(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (command is null)
        {
            return null;
        }

        command.Status = BotControlCommandState.Started;
        command.ClaimedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await NotifyCommandUpdatedAsync(
            dbContext,
            command.CommandId,
            MobileRealtimeEventNames.BotControlCommandStarted,
            cancellationToken);

        return MapToRecord(command);
    }

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
            var command = await db.BotControlCommands
                .FirstAsync(x => x.CommandId == commandId, cancellationToken);

            command.Status = status;
            command.ErrorMessage = errorMessage;
            command.ResultJson = resultJson;
            command.CompletedAtUtc = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
            await NotifyCommandUpdatedAsync(
                db,
                command.CommandId,
                eventName,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static Task NotifyWorkerAsync(
        BotDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return NotifyAsync(dbContext, WorkerWakeUpChannel, null, cancellationToken);
    }

    private static Task NotifyCommandUpdatedAsync(
        BotDbContext dbContext,
        string commandId,
        string eventName,
        CancellationToken cancellationToken)
    {
        var payload = $"{commandId}|{eventName}";
        return NotifyAsync(dbContext, MobileRealtimeCommandUpdatesChannel, payload, cancellationToken);
    }

    private static Task NotifyAsync(
        BotDbContext dbContext,
        string channel,
        string? payload,
        CancellationToken cancellationToken)
    {
        if (!dbContext.Database.IsRelational())
        {
            return Task.CompletedTask;
        }

        return payload is null
            ? dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_notify({channel}, '');",
                cancellationToken)
            : dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_notify({channel}, {payload});",
                cancellationToken);
    }

    private static BotControlCommandRecord MapToRecord(BotControlCommandEntity entity)
    {
        return new BotControlCommandRecord(
            entity.CommandId,
            entity.GuildId,
            entity.UserId,
            entity.Type,
            entity.Status,
            entity.PayloadJson,
            entity.ErrorMessage,
            entity.CreatedAtUtc,
            entity.ClaimedAtUtc,
            entity.CompletedAtUtc,
            entity.ResultJson);
    }
}
