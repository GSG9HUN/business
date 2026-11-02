namespace API.Errors;

/// <summary>
/// Identifies an API failure in a machine-readable form.
/// </summary>
public enum ApiErrorCode
{

    /// <summary>
    /// The failure does not have a more specific API error classification.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound = 1,

    /// <summary>
    /// The request conflicts with the current resource state.
    /// </summary>
    Conflict = 2, // e.g., playlist name conflict

    /// <summary>
    /// The request failed validation.
    /// </summary>
    Validation = 3, // e.g., invalid name

    /// <summary>
    /// The bot is not connected to the required voice channel.
    /// </summary>
    NotConnected = 4, // e.g., Lavalink not connected

    /// <summary>
    /// No active playback player is available.
    /// </summary>
    NoActivePlayer = 5, // e.g., no active player

    /// <summary>
    /// The caller is not permitted to perform the operation.
    /// </summary>
    Forbidden = 6, // e.g., forbidden action

    /// <summary>
    /// The database is unavailable.
    /// </summary>
    DbUnavailable = 7, // e.g., database unavailable

    /// <summary>
    /// The supplied input is invalid.
    /// </summary>
    InvalidInput = 8, // e.g., invalid input provided

    /// <summary>
    /// The request does not provide valid authentication.
    /// </summary>
    Unauthorized = 9,
}
