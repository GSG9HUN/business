namespace DC_bot.Entities.Discovery;

/// <summary>Stores one personal favorite independently of mobile registration and guild membership.</summary>
public sealed class FavoriteTrackEntity
{

    /// <summary>Gets or sets the stable favorite identifier.</summary>
    public long Id { get; set; }

    /// <summary>Gets or sets the owning Discord user.</summary>
    public ulong UserId { get; set; }

    /// <summary>Gets or sets the source/track identity used to prevent duplicate favorites.</summary>
    public string TrackKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the serialized source track.</summary>
    public string TrackIdentifier { get; set; } = string.Empty;

    /// <summary>Gets or sets the display title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the display artist.</summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC creation timestamp.</summary>
    public DateTimeOffset AddedAtUtc { get; set; }
}
