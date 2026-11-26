using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Playback.Models;
using Lavalink4NET.Players;

namespace DC_bot.Interface.Service.Music.Playback;

/// <summary>Composes, confirms, inspects, and restores guild player filters without replacing output volume.</summary>
public interface IPlaybackFilterService
{

    /// <summary>Validates a mobile filter batch before applying it once or saving its draft without changing live filters or output volume.</summary>
    /// <param name="message">Guild and requesting-user context.</param>
    /// <param name="member">Member used by the existing voice-channel authorization boundary.</param>
    /// <param name="commands">Bounded basic/advanced settings or a single preset operation.</param>
    /// <param name="cancellationToken">Cancels supported transport and persistence.</param>
    /// <returns>The authoritative completion, validation failure, or uncertain outcome.</returns>
    Task<PlaybackControlResult> ApplyMobileAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string[] commands,
        CancellationToken cancellationToken = default);


    /// <summary>Shows filters, lists supported presets, applies a preset, or resets the complete filter configuration.</summary>
    /// <param name="message">The message providing guild and response context.</param>
    /// <param name="member">The requesting member, required for mutations.</param>
    /// <param name="arguments">The optional show, list, reset, or preset command arguments.</param>
    /// <param name="cancellationToken">Cancels supported downstream operations.</param>
    /// <returns>The localized confirmed outcome or rejection.</returns>
    Task<PlaybackControlResult> FiltersAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string? arguments,
        CancellationToken cancellationToken = default);

    /// <summary>Shows filters or changes only the equalizer using off, low, medium, or high bass emphasis.</summary>
    /// <param name="message">The guild message receiving the result.</param>
    /// <param name="member">The requesting member, required when changing bass emphasis.</param>
    /// <param name="level">The optional off, low, medium, or high level.</param>
    /// <param name="cancellationToken">Cancels supported downstream operations.</param>
    /// <returns>The localized confirmed configuration or rejection.</returns>
    Task<PlaybackControlResult> BassBoostAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string? level,
        CancellationToken cancellationToken = default);

    /// <summary>Shows or changes speed between 0.5 and 2.0 while retaining pitch, rate, and unrelated filters.</summary>
    /// <param name="message">The guild message receiving the result.</param>
    /// <param name="member">The requesting member, required when changing speed.</param>
    /// <param name="multiplier">The optional invariant multiplier between 0.5 and 2.0.</param>
    /// <param name="cancellationToken">Cancels supported downstream operations.</param>
    /// <returns>The localized speed or confirmed configuration, or a rejection.</returns>
    Task<PlaybackControlResult> SpeedAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string? multiplier,
        CancellationToken cancellationToken = default);

    /// <summary>Shows or changes pitch between 0.5 and 2.0 while retaining speed, rate, and unrelated filters.</summary>
    /// <param name="message">The guild message receiving the result.</param>
    /// <param name="member">The requesting member, required when changing pitch.</param>
    /// <param name="multiplier">The optional invariant multiplier between 0.5 and 2.0.</param>
    /// <param name="cancellationToken">Cancels supported downstream operations.</param>
    /// <returns>The localized pitch or confirmed configuration, or a rejection.</returns>
    Task<PlaybackControlResult> PitchAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string? multiplier,
        CancellationToken cancellationToken = default);

    /// <summary>Restores the last confirmed process-local filters during owned join validation before exposing the player.</summary>
    /// <param name="player">The candidate player owned by the active join lifecycle.</param>
    /// <param name="cancellationToken">Cancels supported downstream operations; join cleanup remains owned by the caller.</param>
    /// <returns>A task completing after applicable filters are acknowledged and marked recovered.</returns>
    Task RestoreAsync(ILavalinkPlayer player, CancellationToken cancellationToken = default);
}
