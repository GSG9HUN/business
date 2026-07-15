using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Persistence.Repositories;

namespace DC_bot_tests.UnitTests.Persistence;

[Trait("Category", "Unit")]
public class PostgreSqlQueueClaimSqlTests
{
    [Fact]
    public void ClaimNextQueuedItem_ReturnsParameterizedSkipLockedQuery()
    {
        const ulong guildId = 42ul;

        var sql = PostgreSqlQueueClaimSql.ClaimNextQueuedItem(guildId);

        Assert.Contains("FOR UPDATE SKIP LOCKED", sql.Format, StringComparison.Ordinal);
        Assert.Contains(@"""guild_queue_item""", sql.Format, StringComparison.Ordinal);
        var arguments = sql.GetArguments();
        Assert.Equal(guildId, arguments[0]);
        Assert.Equal((short)QueueItemState.Queued, arguments[1]);
    }
}
