using DC_bot.Db;
using DC_bot.Entities.BotControl;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.BotControl;

public class BotControlCommandsRepository(IDbContextFactory<BotDbContext> dbContextFactory): IBotControlCommandsRepository
{
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

    public async Task<BotControlCommandRecord> EnqueueAsync(ulong guildId, ulong userId, string type, CancellationToken cancellationToken)
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
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
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
            await transaction.CommitAsync(cancellationToken);
        
            return MapToRecord(botCommand);
        }
        catch (Exception)
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

    public Task MarkDoneAsync(string commandId, CancellationToken cancellationToken)
    {
        return UpdateStatusAsync(commandId, BotControlCommandState.Done, null, cancellationToken);
    }

    public Task MarkFailedAsync(string commandId, string errorMessage, CancellationToken cancellationToken)
    {
        return UpdateStatusAsync(commandId, BotControlCommandState.Failed, errorMessage, cancellationToken);
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

        return MapToRecord(command);
    }

    private async Task UpdateStatusAsync(
        string commandId,
        BotControlCommandState status,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var command = await db.BotControlCommands
                .FirstAsync(x => x.CommandId == commandId, cancellationToken);

            command.Status = status;
            command.ErrorMessage = errorMessage;
            command.CompletedAtUtc = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception )
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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
            entity.CompletedAtUtc);
    }
}
