namespace DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

public enum BotControlCommandState : short
{
    Pending = 0,
    Started = 1,
    Done = 2,
    Failed = 3
}
