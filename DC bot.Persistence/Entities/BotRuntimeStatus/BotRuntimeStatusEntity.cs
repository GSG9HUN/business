namespace DC_bot.Entities.BotRuntimeStatus;

public class BotRuntimeStatusEntity
{
    public int Id { get; set; } 
    public DateTimeOffset LastHeartbeatAtUtc { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
}