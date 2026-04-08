using DC_bot.Persistence.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DC_bot_tests.UnitTests.Persistence;

/// <summary>
/// Creates a fresh in-memory BotDbContext for each call, isolated per test.
/// </summary>
internal sealed class InMemoryDbContextFactory : IDbContextFactory<BotDbContext>
{
    private readonly DbContextOptions<BotDbContext> _options;

    public InMemoryDbContextFactory(string databaseName)
    {
        _options = new DbContextOptionsBuilder<BotDbContext>()
            .UseInMemoryDatabase(databaseName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    public BotDbContext CreateDbContext() => new(_options);
}

