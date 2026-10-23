using DC_bot.Helper.Validation;
using DC_bot.Interface.Discord;

namespace DC_bot.Interface.Core;

/// <summary>
/// Defines the user validation service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IUserValidationService
{

    /// <summary>
    /// Checks that the requesting Discord user may perform voice-bound music operations.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<UserValidationResult> ValidateUserAsync(IDiscordMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether an event user is the currently authenticated bot.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    public bool IsBotUser(IDiscordMessage message);
}
