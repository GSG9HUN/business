using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Persistence.Db;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Persistence.Repositories;

internal sealed class QueueClaimService(IDbContextFactory<BotDbContext> dbContextFactory)
{
    public async Task<QueueItemRecord?> ClaimNextQueuedItemAsync(
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var entity = await dbContext.GuildQueueItems
                .FromSqlInterpolated(PostgreSqlQueueClaimSql.ClaimNextQueuedItem(guildId))
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null) return null;

            entity.State = QueueItemState.Playing;
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return QueueItemMapper.ToRecord(entity);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
