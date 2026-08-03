namespace DC_bot.Interface.Service.Persistence.Models;

public enum QueueItemState : short
{
    Queued = 0,
    Playing = 1,
    Played = 2,
    Skipped = 3
}
