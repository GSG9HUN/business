using DC_bot.Db;
using DC_bot.Entities.MobileApps;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Models.MobileApps;
using Microsoft.EntityFrameworkCore;
using DC_bot.Repositories.Shared;

namespace DC_bot.Repositories.MobileApps;

/// <summary>
/// Reads and mutates mobile app session state using short-lived contexts from the injected EF factory.
/// </summary>
/// <param name = "dbContextFactory">The injected factory used to create a fresh context for each operation or transaction attempt.</param>
public class MobileAppSessionRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IMobileAppSessionRepository
{

    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid sessionId,
        ulong discordUserId,
        string refreshTokenHash,
        DateTimeOffset expiresAtUtc,
        CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            db.MobileAppSessions.Add(new MobileAppSessionEntity { SessionId = sessionId, DiscordUserId = discordUserId, RefreshTokenHash = refreshTokenHash, CreatedAtUtc = DateTimeOffset.UtcNow, LastRefreshedAtUtc = DateTimeOffset.UtcNow, RefreshTokenExpiresAtUtc = expiresAtUtc });
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception operationException)
        {
            await TransactionCleanup.TryRollbackAsync(transaction, operationException);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<MobileAppSessionRecord?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        return await db.MobileAppSessions.AsNoTracking()
            .Where(x => x.RefreshTokenHash == refreshTokenHash)
            .Select(x => new MobileAppSessionRecord(x.SessionId, x.DiscordUserId, x.RefreshTokenHash, x.RefreshTokenExpiresAtUtc, x.RevokedAtUtc))
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> RotateRefreshTokenAsync(
        Guid sessionId,
        string currentRefreshTokenHash,
        string newRefreshTokenHash,
        DateTimeOffset expiresAtUtc,
        CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var now = DateTimeOffset.UtcNow;
        var affectedRows = await db.MobileAppSessions.Where(x => x.SessionId == sessionId && x.RefreshTokenHash == currentRefreshTokenHash && x.RevokedAtUtc == null && x.RefreshTokenExpiresAtUtc > now)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RefreshTokenHash, newRefreshTokenHash)
                .SetProperty(x => x.RefreshTokenExpiresAtUtc, expiresAtUtc)
                .SetProperty(x => x.LastRefreshedAtUtc, now), ct);
        return affectedRows == 1;
    }

    /// <inheritdoc/>
    public async Task RevokeAsync(Guid sessionId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var session = await db.MobileAppSessions.FirstAsync(x => x.SessionId == sessionId, ct);
            session.RevokedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception operationException)
        {
            await TransactionCleanup.TryRollbackAsync(transaction, operationException);
            throw;
        }
    }
}
