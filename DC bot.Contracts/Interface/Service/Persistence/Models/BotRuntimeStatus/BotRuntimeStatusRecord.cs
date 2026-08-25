namespace DC_bot.Interface.Service.Persistence.Models.BotRuntimeStatus;

public record BotRuntimeStatusRecord(int Id, DateTimeOffset LastHeartbeatAtUtc, DateTimeOffset StartedAtUtc);