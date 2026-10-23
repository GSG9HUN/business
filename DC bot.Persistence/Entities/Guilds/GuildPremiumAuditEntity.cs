namespace DC_bot.Entities.Guilds;

/// <summary>
/// Represents the persisted guild premium audit row mapped by the EF Core model.
/// </summary>
public class GuildPremiumAuditEntity
{

    /// <summary>
    /// The identifier of this persisted record.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The Discord guild identifier.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    /// The Discord identifier of the user who changed the premium status.
    /// </summary>
    public long? ChangedByUserId { get; set; }

    /// <summary>
    /// The premium flag before the change.
    /// </summary>
    public bool OldIsPremium { get; set; }

    /// <summary>
    /// The premium flag after the change.
    /// </summary>
    public bool NewIsPremium { get; set; }

    /// <summary>
    /// The UTC time at which the premium status changed.
    /// </summary>
    public DateTimeOffset ChangedAtUtc { get; set; }

    /// <summary>
    /// The optional explanation recorded with the premium status change.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// The guild associated with this object.
    /// </summary>
    public GuildDataEntity Guild { get; set; } = null !;
}
