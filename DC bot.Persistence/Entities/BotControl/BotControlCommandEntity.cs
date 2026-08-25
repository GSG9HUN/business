using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

namespace DC_bot.Entities.BotControl;

public class BotControlCommandEntity
{
    public string CommandId { get; set; } = string.Empty;
    public ulong GuildId { get; set; }
    public string Type { get; set; } = string.Empty;
    public ulong UserId { get; set; }
    public BotControlCommandState Status { get; set; } = BotControlCommandState.Pending;
    
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ClaimedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
}