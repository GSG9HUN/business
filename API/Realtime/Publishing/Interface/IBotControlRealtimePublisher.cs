using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

namespace API.Realtime.Publishing.Interface;

public interface IBotControlRealtimePublisher
{
    Task PublishCommandAsync(
        BotControlCommandRecord command,
        string eventName,
        CancellationToken cancellationToken = default);
}