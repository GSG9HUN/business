using DC_bot.Db;
using DC_bot.Entities;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories;

public class BotControlCommandsRepository(IDbContextFactory<BotDbContext> dbContextFactory): IBotControlCommandsRepository
{
    public async Task<BotControlCommandRecord> EnqueueAsync(ulong guildId, ulong userId, string type, CancellationToken cancellationToken)
    {
        
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        
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
        
        return new BotControlCommandRecord(
            botCommand.CommandId,
            botCommand.GuildId,
            botCommand.UserId,
            botCommand.Type,
            botCommand.Status
        );
    }

    public Task<BotControlCommandRecord?> ClaimNextPendingAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task MarkDoneAsync(string commandId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task MarkFailedAsync(string commandId, string errorMessage, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}