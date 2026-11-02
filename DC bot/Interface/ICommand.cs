using DC_bot.Interface.Discord;

namespace DC_bot.Interface;

/// <summary>
/// Defines the command boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ICommand
{

    /// <summary>
    /// The display name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The localized command description shown by help output.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Executes the registered text command using the supplied message and its response boundary.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task ExecuteAsync(IDiscordMessage message);
}
