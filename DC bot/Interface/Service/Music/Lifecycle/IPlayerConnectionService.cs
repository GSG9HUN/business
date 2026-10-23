using DC_bot.Interface.Discord;
using Lavalink4NET.Players;

namespace DC_bot.Interface.Service.Music.Lifecycle;

/// <summary>
/// Defines the player connection service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IPlayerConnectionService
{

    /// <summary>
    /// Joins or resolves the voice player and waits for readiness within the caller&apos;s join deadline.
    /// </summary>
    /// <remarks>Cancellation bounds waiting, not the remote effects of an already-dispatched SDK join. Late completion remains observed and subject to cleanup.</remarks>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="channel">The discord channel? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<(ILavalinkPlayer? connection, IDiscordChannel? channel, ulong guildId, bool isValid)> TryJoinAndValidateAsync(
        IDiscordMessage message,
        IDiscordChannel? channel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves and validates the existing player without creating a voice connection.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="channel">The discord channel? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<(ILavalinkPlayer? connection, IDiscordChannel? channel, ulong guildId, bool isValid)> TryGetAndValidateExistingPlayerAsync(
        IDiscordMessage message,
        IDiscordChannel? channel,
        CancellationToken cancellationToken = default);
}
