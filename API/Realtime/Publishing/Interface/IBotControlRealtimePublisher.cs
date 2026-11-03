using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

namespace API.Realtime.Publishing.Interface;

/// <summary>
/// Defines publication of durable command progress to mobile subscribers.
/// </summary>
public interface IBotControlRealtimePublisher
{

    /// <summary>
    /// Sends command progress to its owner&apos;s guild group and emits the generic command event when different.
    /// </summary>
    /// <param name="command">The command value for this operation.</param>
    /// <param name="eventName">The event name value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
    Task PublishCommandAsync(BotControlCommandRecord command, string eventName, CancellationToken cancellationToken = default);
}
