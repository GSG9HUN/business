using DC_bot.Interface.Service.Presentation.Models;

namespace DC_bot.Interface.Service.Music.Playback.Models;

/// <summary>
/// Carries the success state and response details of a playback control.
/// </summary>
/// <param name="Success">The success supplied for this operation.</param>
/// <param name="Message">The message supplied for this operation.</param>
/// <param name="ErrorCode">The error code supplied for this operation.</param>
public sealed record PlaybackControlResult(bool Success, string Message, string? ErrorCode = null)
{

    /// <summary>Gets whether a successful result only describes state rather than confirming a change.</summary>
    public bool IsInformational { get; init; }

    /// <summary>Gets whether a completed operation has a caveat or a partially saved outcome.</summary>
    public bool HasWarning { get; init; }

    /// <summary>Gets an optional localized outcome title selected without inspecting formatted response text.</summary>
    public string? ResponseTitleKey { get; init; }

    /// <summary>Creates a successful read-only result with informational presentation.</summary>
    public static PlaybackControlResult Information(string message)
    {
        return new PlaybackControlResult(true, message) { IsInformational = true };
    }

    /// <summary>Creates a completed result whose caveat must be presented as a warning.</summary>
    public static PlaybackControlResult Warning(string message)
    {
        return new PlaybackControlResult(true, message) { HasWarning = true };
    }

    /// <summary>Gets optional read-only pagination for Discord presentation; ordinary bridge text remains in Message.</summary>
    public PaginationRequest? Pagination { get; init; }

    /// <summary>Gets the author and title captured from the track targeted by a successful control operation.</summary>
    public string? TrackDisplayName { get; init; }

    /// <summary>
    /// Creates a successful playback-control result.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    public static PlaybackControlResult Succeeded(string message)
    {
        return new PlaybackControlResult(true, message);
    }

    /// <summary>
    /// Creates a failed playback-control result with response details.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <param name="errorCode">The error code supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    public static PlaybackControlResult Failed(string message, string errorCode)
    {
        return new PlaybackControlResult(false, message, errorCode);
    }
}
