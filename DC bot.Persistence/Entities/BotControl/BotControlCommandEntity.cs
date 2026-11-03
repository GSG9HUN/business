using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

namespace DC_bot.Entities.BotControl;

/// <summary>
/// Represents the persisted bot control command row mapped by the EF Core model.
/// </summary>
public class BotControlCommandEntity
{

    /// <summary>
    /// The identifier used to correlate the command and its execution result.
    /// </summary>
    public string CommandId { get; set; } = string.Empty;

    /// <summary>
    /// The Discord guild identifier.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    /// The command type dispatched to the bot.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The serialized command arguments, or null when no payload was supplied.
    /// </summary>
    public string? PayloadJson { get; set; }

    /// <summary>
    /// The identifier of the user who requested the command.
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    /// The serialized execution result, or null before a result is available.
    /// </summary>
    public string? ResultJson { get; set; }

    /// <summary>
    /// The persisted command execution state.
    /// </summary>
    public BotControlCommandState Status { get; set; } = BotControlCommandState.Pending;

    /// <summary>
    /// The failure description, or null when no error was recorded.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The UTC time at which this record was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// The UTC time at which a worker claimed the command, or null before it is claimed.
    /// </summary>
    public DateTimeOffset? ClaimedAtUtc { get; set; }

    /// <summary>
    /// The UTC completion time, or null while the operation remains open.
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; set; }
}
