using DC_bot.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DC_bot_tests.UnitTests.Persistence;

public class GuildPremiumAuditEntityTests
{
    private static InMemoryDbContextFactory CreateFactory() =>
        new($"PremiumAudit_{Guid.NewGuid()}");

    private static async Task SeedGuildAsync(InMemoryDbContextFactory factory, long guildId)
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
        await SeedGuildAsync(factory, 1L);

        await using (var db = factory.CreateDbContext())
        {
            db.GuildPremiumAudits.Add(new GuildPremiumAuditEntity
            {
                GuildId = 1L,
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

        Assert.Equal(1L, entry.GuildId);
        Assert.Equal(42L, entry.ChangedByUserId);
        Assert.False(entry.OldIsPremium);
        Assert.True(entry.NewIsPremium);
        Assert.Equal("test note", entry.Note);
    }

    [Fact]
    public async Task GuildPremiumAuditEntity_NullableFields_CanBeNull()
    {
        var factory = CreateFactory();
        await SeedGuildAsync(factory, 2L);

        await using (var db = factory.CreateDbContext())
        {
            db.GuildPremiumAudits.Add(new GuildPremiumAuditEntity
            {
                GuildId = 2L,
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
        await SeedGuildAsync(factory, 3L);

        await using (var db = factory.CreateDbContext())
        {
            db.GuildPremiumAudits.AddRange(
                new GuildPremiumAuditEntity { GuildId = 3L, OldIsPremium = false, NewIsPremium = true, ChangedAtUtc = DateTimeOffset.UtcNow },
                new GuildPremiumAuditEntity { GuildId = 3L, OldIsPremium = true, NewIsPremium = false, ChangedAtUtc = DateTimeOffset.UtcNow.AddMinutes(1) }
            );
            await db.SaveChangesAsync();
        }

        await using var verify = factory.CreateDbContext();
        Assert.Equal(2, verify.GuildPremiumAudits.Count(a => a.GuildId == 3L));
    }

    [Fact]
    public async Task GuildPremiumAuditEntity_WhenGuildDeleted_AuditEntriesCascadeDelete()
    {
        var factory = CreateFactory();
        await SeedGuildAsync(factory, 4L);

        await using (var db = factory.CreateDbContext())
        {
            db.GuildPremiumAudits.Add(new GuildPremiumAuditEntity
            {
                GuildId = 4L,
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
                .Single(g => g.GuildId == 4L);
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
        await SeedGuildAsync(factory, 5L);

        await using (var db = factory.CreateDbContext())
        {
            db.GuildPremiumAudits.Add(new GuildPremiumAuditEntity
            {
                GuildId = 5L,
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

