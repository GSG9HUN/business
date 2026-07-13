using DC_bot.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DC_bot_tests.UnitTests.Persistence;

[Trait("Category", "Unit")]
public class GuildPremiumAuditEntityTests
{
    private static InMemoryDbContextFactory CreateFactory() =>
        new($"PremiumAudit_{Guid.NewGuid()}");

    private static async Task SeedGuildAsync(InMemoryDbContextFactory factory, ulong guildId)
    {
        await using var db = factory.CreateDbContext();
        db.GuildData.Add(new GuildDataEntity
        {
            GuildId = guildId,
            IsPremium = false,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GuildPremiumAuditEntity_CanBeSavedAndRetrieved()
    {
        var factory = CreateFactory();
        await SeedGuildAsync(factory, 1UL);

        await using (var db = factory.CreateDbContext())
        {
            db.GuildPremiumAudits.Add(new GuildPremiumAuditEntity
            {
                GuildId = 1UL,
                ChangedByUserId = 42L,
                OldIsPremium = false,
                NewIsPremium = true,
                ChangedAtUtc = DateTimeOffset.UtcNow,
                Note = "test note"
            });
            await db.SaveChangesAsync();
        }

        await using var verify = factory.CreateDbContext();
        var entry = verify.GuildPremiumAudits.Single();

        Assert.Equal(1UL, entry.GuildId);
        Assert.Equal(42L, entry.ChangedByUserId);
        Assert.False(entry.OldIsPremium);
        Assert.True(entry.NewIsPremium);
        Assert.Equal("test note", entry.Note);
    }

    [Fact]
    public async Task GuildPremiumAuditEntity_NullableFields_CanBeNull()
    {
        var factory = CreateFactory();
        await SeedGuildAsync(factory, 2UL);

        await using (var db = factory.CreateDbContext())
        {
            db.GuildPremiumAudits.Add(new GuildPremiumAuditEntity
            {
                GuildId = 2UL,
                ChangedByUserId = null,
                OldIsPremium = true,
                NewIsPremium = false,
                ChangedAtUtc = DateTimeOffset.UtcNow,
                Note = null
            });
            await db.SaveChangesAsync();
        }

        await using var verify = factory.CreateDbContext();
        var entry = verify.GuildPremiumAudits.Single();

        Assert.Null(entry.ChangedByUserId);
        Assert.Null(entry.Note);
    }

    [Fact]
    public async Task GuildPremiumAuditEntity_MultipleEntriesForSameGuild_AllPersisted()
    {
        var factory = CreateFactory();
        await SeedGuildAsync(factory, 3UL);

        await using (var db = factory.CreateDbContext())
        {
            db.GuildPremiumAudits.AddRange(
                new GuildPremiumAuditEntity { GuildId = 3UL, OldIsPremium = false, NewIsPremium = true, ChangedAtUtc = DateTimeOffset.UtcNow },
                new GuildPremiumAuditEntity { GuildId = 3UL, OldIsPremium = true, NewIsPremium = false, ChangedAtUtc = DateTimeOffset.UtcNow.AddMinutes(1) }
            );
            await db.SaveChangesAsync();
        }

        await using var verify = factory.CreateDbContext();
        Assert.Equal(2, verify.GuildPremiumAudits.Count(a => a.GuildId == 3UL));
    }

    [Fact]
    public async Task GuildPremiumAuditEntity_WhenGuildDeleted_AuditEntriesCascadeDelete()
    {
        var factory = CreateFactory();
        await SeedGuildAsync(factory, 4UL);

        await using (var db = factory.CreateDbContext())
        {
            db.GuildPremiumAudits.Add(new GuildPremiumAuditEntity
            {
                GuildId = 4UL,
                OldIsPremium = false,
                NewIsPremium = true,
                ChangedAtUtc = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }
        await using (var db = factory.CreateDbContext())
        {
            var guild = db.GuildData
                .Include(g => g.PremiumAuditEntries)
                .Single(g => g.GuildId == 4UL);
            db.GuildData.Remove(guild);
            await db.SaveChangesAsync();
        }

        await using var verify = factory.CreateDbContext();
        Assert.Empty(verify.GuildPremiumAudits);
    }

    [Fact]
    public async Task GuildPremiumAuditEntity_IdIsAutoAssigned()
    {
        var factory = CreateFactory();
        await SeedGuildAsync(factory, 5UL);

        await using (var db = factory.CreateDbContext())
        {
            db.GuildPremiumAudits.Add(new GuildPremiumAuditEntity
            {
                GuildId = 5UL,
                OldIsPremium = false,
                NewIsPremium = true,
                ChangedAtUtc = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        await using var verify = factory.CreateDbContext();
        var entry = verify.GuildPremiumAudits.Single();
        Assert.NotEqual(0L, entry.Id);
    }
}

