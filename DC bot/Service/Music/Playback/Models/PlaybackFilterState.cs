using Lavalink4NET.Protocol.Models.Filters;

namespace DC_bot.Service.Music.Playback.Models;

/// <summary>Retains a complete immutable filter snapshot independently of the mutable SDK filter collection.</summary>
/// <param name="Filters">The typed built-in filters and preserved plugin filter values for one guild player.</param>
public sealed record PlaybackFilterState(PlayerFilterMapModel Filters);
