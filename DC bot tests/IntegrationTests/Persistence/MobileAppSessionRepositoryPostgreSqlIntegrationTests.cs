using DC_bot.Db;
using DC_bot.Entities.MobileApps;
using DC_bot.Repositories.MobileApps;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Persistence;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class MobileAppSessionRepositoryPostgreSqlIntegrationTests
{
    [Fact]
    public async Task RotateRefreshTokenAsync_WhenOldTokenIsUsedConcurrently_AllowsOnlyOneRotation()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var factory = services.GetRequiredService<IDbContextFactory<BotDbContext>>();
        var repository = new MobileAppSessionRepository(factory);
        var sessionId = Guid.NewGuid();
        const string oldRefreshTokenHash = "old-token-hash";
        const string firstNewRefreshTokenHash = "new-token-hash-a";
        const string secondNewRefreshTokenHash = "new-token-hash-b";
        const ulong discordUserId = 42ul;

        await SeedMobileAppUserAsync(factory, discordUserId);

        await repository.CreateAsync(
            sessionId,
            discordUserId,
            oldRefreshTokenHash,
            DateTimeOffset.UtcNow.AddDays(1));

        var rotations = await Task.WhenAll(
            new MobileAppSessionRepository(factory).RotateRefreshTokenAsync(
                sessionId,
                oldRefreshTokenHash,
                firstNewRefreshTokenHash,
                DateTimeOffset.UtcNow.AddDays(30)),
            new MobileAppSessionRepository(factory).RotateRefreshTokenAsync(
                sessionId,
                oldRefreshTokenHash,
                secondNewRefreshTokenHash,
                DateTimeOffset.UtcNow.AddDays(30)));

        Assert.Single(rotations, rotated => rotated);
        Assert.Null(await repository.GetByRefreshTokenHashAsync(oldRefreshTokenHash));
        Assert.True(await HasSessionWithRefreshTokenHashAsync(factory, firstNewRefreshTokenHash) ^
                    await HasSessionWithRefreshTokenHashAsync(factory, secondNewRefreshTokenHash));
    }

    private async Task SeedMobileAppUserAsync(IDbContextFactory<BotDbContext> factory, ulong discordUserId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        if (!await dbContext.MobileAppUsers.AnyAsync(u => u.DiscordUserId == discordUserId))
        {
            dbContext.MobileAppUsers.Add(new MobileAppUserEntity
            {
                DiscordUserId = discordUserId,
                Username = "testuser",
                GlobalName = null,
                AvatarHash = null,
            });
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task<bool> HasSessionWithRefreshTokenHashAsync(
        IDbContextFactory<BotDbContext> factory,
        string refreshTokenHash)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        return await dbContext.MobileAppSessions.AnyAsync(session => session.RefreshTokenHash == refreshTokenHash);
    }
}