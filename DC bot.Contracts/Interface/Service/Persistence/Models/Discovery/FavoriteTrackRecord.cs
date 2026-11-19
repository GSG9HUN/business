namespace DC_bot.Interface.Service.Persistence.Models.Discovery;

/// <summary>Carries a user-owned favorite across the storage boundary.</summary>
/// <param name="Id">The stable favorite identifier.</param>
/// <param name="TrackIdentifier">The serialized source track.</param>
/// <param name="Title">The display title.</param>
/// <param name="Author">The display artist.</param>
public sealed record FavoriteTrackRecord(long Id, string TrackIdentifier, string Title, string Author);
