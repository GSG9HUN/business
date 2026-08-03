using DC_bot.Interface.Service.Persistence.Models;

namespace DC_bot.Entities;

public class BotControlCommandEntity
{
    public string CommandId { get; set; } = string.Empty;
    public ulong GuildId { get; set; }
    public string Type { get; set; } = string.Empty;
    public ulong UserId { get; set; }
    public BotControlCommandState Status { get; set; } = BotControlCommandState.Pending;
}