using DC_bot.Db;
using DC_bot.Interface.Service.Persistence.Status;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Status;

/// <summary>
/// Probes database connectivity without converting requested cancellation into a connectivity failure.
/// </summary>
/// <param name = "dbContextFactory">The injected factory used to create a fresh context for each operation or transaction attempt.</param>
public class DbStatusCheck(IDbContextFactory<BotDbContext> dbContextFactory) : IDbStatusCheck
{

    /// <inheritdoc/>
    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var connected = await dbContext.Database.CanConnectAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return connected;
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }
}
