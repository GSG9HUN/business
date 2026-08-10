using DC_bot.Interface.Service.Persistence.Models.Queue;

namespace DC_bot.Repositories.Queue;

public static class PostgreSqlQueueClaimSql
{
    public static FormattableString ClaimNextQueuedItem(ulong guildId)
    {
        var queuedState = (short)QueueItemState.Queued;

        return $@"
                SELECT * FROM ""guild_queue_item""
                WHERE ""guild_id"" = {guildId} AND ""state"" = {queuedState}
                ORDER BY ""position""
                LIMIT 1
                FOR UPDATE SKIP LOCKED";
    }
}
