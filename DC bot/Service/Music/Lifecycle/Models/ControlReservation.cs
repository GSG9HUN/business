namespace DC_bot.Service.Music.Lifecycle.Models;

/// <summary>
/// Identifies the operation that exclusively owns playback control in one guild generation.
/// </summary>
/// <param name="GuildId">The Discord guild whose state or operation is addressed.</param>
/// <param name="Generation">The guild lifecycle generation used to reject stale work.</param>
/// <param name="OperationId">The correlation identifier of the playback or command operation.</param>
/// <param name="OperationName">The operation name supplied for this operation.</param>
public sealed record ControlReservation(ulong GuildId, long Generation, Guid OperationId, string OperationName);
