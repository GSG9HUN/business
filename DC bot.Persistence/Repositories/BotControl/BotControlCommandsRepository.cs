using DC_bot.Db;
using DC_bot.Entities.BotControl;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.BotControl;

public class BotControlCommandsRepository(IDbContextFactory<BotDbContext> dbContextFactory): IBotControlCommandsRepository
{
    public async Task<BotControlCommandRecord> EnqueueAsync(ulong guildId, ulong userId, string type, CancellationToken cancellationToken)
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
                UserId = userId,
                Status = BotControlCommandState.Pending
            };
        
            dbContext.BotControlCommands.Add(botCommand);
        
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        
            return new BotControlCommandRecord(
                botCommand.CommandId,
                botCommand.GuildId,
                botCommand.UserId,
                botCommand.Type,
                botCommand.Status
            );
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public Task<BotControlCommandRecord?> ClaimNextPendingAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task MarkDoneAsync(string commandId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task MarkFailedAsync(string commandId, string errorMessage, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var command = await db.BotControlCommands
                .FirstAsync(x => x.CommandId == commandId, cancellationToken);

            command.Status = BotControlCommandState.Failed;
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
}