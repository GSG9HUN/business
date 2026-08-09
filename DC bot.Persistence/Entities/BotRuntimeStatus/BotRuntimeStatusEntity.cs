namespace DC_bot.Entities.BotRuntimeStatus;

public class BotStatusRuntimeEntity
{
    public int Id { get; set; } 
    public DateTimeOffset LastHeartbeatAtUtc { get; set; }
    public DateTimeOffset LastStatusChangeAtUtc { get; set; }
}