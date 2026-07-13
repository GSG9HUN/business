using DC_bot.Persistence.Db;
using DC_bot_tests.TestHelperFiles;
using Microsoft.EntityFrameworkCore;

namespace DC_bot_tests.UnitTests.Persistence;

[Trait("Category", "Unit")]
public class BotDbContextFactoryTests
{
    [Fact]
    public void CreateDbContext_WhenEnvironmentVariablesAreMissing_UsesDefaultConnectionString()
    {
        using var _ = new TestEnvironmentVariableScope(new Dictionary<string, string?>
        {
            ["POSTGRES_HOST"] = null,
            ["POSTGRES_PORT"] = null,
            ["POSTGRES_DB"] = null,
            ["POSTGRES_USER"] = null,
            ["POSTGRES_PASSWORD"] = null
        });

        using var dbContext = new BotDbContextFactory().CreateDbContext([]);

        Assert.Equal(
            "Host=localhost;Port=5432;Database=dc_bot;Username=postgres;Password=postgres",
            dbContext.Database.GetConnectionString());
    }

    [Fact]
    public void CreateDbContext_WhenEnvironmentVariablesAreQuoted_TrimsValues()
    {
        using var _ = new TestEnvironmentVariableScope(new Dictionary<string, string?>
        {
            ["POSTGRES_HOST"] = " \"db-host\" ",
            ["POSTGRES_PORT"] = " \"15432\" ",
            ["POSTGRES_DB"] = " \"music\" ",
            ["POSTGRES_USER"] = " \"bot\" ",
            ["POSTGRES_PASSWORD"] = " \"secret\" "
        });

        using var dbContext = new BotDbContextFactory().CreateDbContext([]);

        Assert.Equal(
            "Host=db-host;Port=15432;Database=music;Username=bot;Password=secret",
            dbContext.Database.GetConnectionString());
    }
}
