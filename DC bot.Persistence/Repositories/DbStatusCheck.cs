using DC_bot.Db;
using DC_bot.Interface.Service.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories;

public class DbStatusCheck(IDbContextFactory<BotDbContext> dbContextFactory): IDbStatusCheck
{
    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await dbContext.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    
    }
}