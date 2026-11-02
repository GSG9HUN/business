namespace DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

/// <summary>
/// Identifies the persisted execution stage of a bot control command.
/// </summary>
public enum BotControlCommandState : short
{

    /// <summary>
    /// The command is waiting to be claimed by a worker.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// A worker has claimed the command and started executing it.
    /// </summary>
    Started = 1,

    /// <summary>
    /// The command completed successfully.
    /// </summary>
    Done = 2,

    /// <summary>
    /// The operation failed.
    /// </summary>
    Failed = 3
}
