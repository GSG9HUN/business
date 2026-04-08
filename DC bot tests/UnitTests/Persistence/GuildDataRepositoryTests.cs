using DC_bot.Persistence.Repositories;

namespace DC_bot_tests.UnitTests.Persistence;

public class GuildDataRepositoryTests
{
    private static InMemoryDbContextFactory CreateFactory() =>
        new($"GuildData_{Guid.NewGuid()}");

    [Fact]
    public async Task EnsureGuildExistsAsync_WhenGuildDoesNotExist_CreatesGuild()
    {
        var factory = CreateFactory();
        var repo = new GuildDataRepository(factory);

        await repo.EnsureGuildExistsAsync(1111ul);

        await using var db = factory.CreateDbContext();
        Assert.Single(db.GuildData);
        Assert.Equal(1111L, db.GuildData.First().GuildId);
    }

    [Fact]
    public async Task EnsureGuildExistsAsync_WhenGuildAlreadyExists_DoesNotCreateDuplicate()
    {
        var factory = CreateFactory();
        var repo = new GuildDataRepository(factory);

        await repo.EnsureGuildExistsAsync(1111ul);
        await repo.EnsureGuildExistsAsync(1111ul);

        await using var db = factory.CreateDbContext();
        Assert.Single(db.GuildData);
    }

    [Fact]
    public async Task IsPremiumAsync_WhenGuildDoesNotExist_ReturnsFalse()
    {
        var repo = new GuildDataRepository(CreateFactory());

        var result = await repo.IsPremiumAsync(9999ul);

        Assert.False(result);
    }

    [Fact]
    public async Task IsPremiumAsync_WhenGuildIsPremiumWithNoExpiry_ReturnsTrue()
    {
        var factory = CreateFactory();
        var repo = new GuildDataRepository(factory);

        await repo.UpsertPremiumAsync(1ul, isPremium: true, premiumUntilUtc: null);

        Assert.True(await repo.IsPremiumAsync(1ul));
    }

    [Fact]
    public async Task IsPremiumAsync_WhenPremiumExpiryIsInTheFuture_ReturnsTrue()
    {
        var factory = CreateFactory();
        var repo = new GuildDataRepository(factory);

        await repo.UpsertPremiumAsync(2ul, isPremium: true, premiumUntilUtc: DateTimeOffset.UtcNow.AddDays(1));

        Assert.True(await repo.IsPremiumAsync(2ul));
    }

    [Fact]
    public async Task IsPremiumAsync_WhenPremiumExpiryIsInThePast_ReturnsFalse()
    {
        var factory = CreateFactory();
        var repo = new GuildDataRepository(factory);

        await repo.UpsertPremiumAsync(3ul, isPremium: true, premiumUntilUtc: DateTimeOffset.UtcNow.AddDays(-1));

        Assert.False(await repo.IsPremiumAsync(3ul));
    }

    [Fact]
    public async Task IsPremiumAsync_WhenIsPremiumIsFalse_ReturnsFalse()
    {
        var factory = CreateFactory();
        var repo = new GuildDataRepository(factory);

        await repo.UpsertPremiumAsync(4ul, isPremium: false, premiumUntilUtc: null);

        Assert.False(await repo.IsPremiumAsync(4ul));
    }

    [Fact]
    public async Task UpsertPremiumAsync_WhenGuildDoesNotExist_CreatesRecord()
    {
        var factory = CreateFactory();
        var repo = new GuildDataRepository(factory);

        await repo.UpsertPremiumAsync(5ul, isPremium: true, premiumUntilUtc: null);

        await using var db = factory.CreateDbContext();
        var entity = db.GuildData.Single(g => g.GuildId == 5L);
        Assert.True(entity.IsPremium);
        Assert.Null(entity.PremiumUntilUtc);
    }

    [Fact]
    public async Task UpsertPremiumAsync_WhenGuildAlreadyExists_UpdatesRecord()
    {
        var factory = CreateFactory();
        var repo = new GuildDataRepository(factory);
        var expiry = DateTimeOffset.UtcNow.AddDays(30);

        await repo.UpsertPremiumAsync(6ul, isPremium: false, premiumUntilUtc: null);
        await repo.UpsertPremiumAsync(6ul, isPremium: true, premiumUntilUtc: expiry);

        await using var db = factory.CreateDbContext();
        var entity = db.GuildData.Single(g => g.GuildId == 6L);
        Assert.True(entity.IsPremium);
        Assert.NotNull(entity.PremiumUntilUtc);
    }
}

