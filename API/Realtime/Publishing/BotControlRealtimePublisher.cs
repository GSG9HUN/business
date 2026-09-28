using API.Realtime.Events;
using API.Realtime.Publishing.Interface;
using DC_bot.BotControl;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;
using Microsoft.AspNetCore.SignalR;

namespace API.Realtime.Publishing;

public class BotControlRealtimePublisher(
    IHubContext<MobileUpdatesHub> hubContext,
    ILogger<BotControlRealtimePublisher> logger) : IBotControlRealtimePublisher
{
  public async Task PublishCommandAsync(
        BotControlCommandRecord command,
        string eventName,
        CancellationToken cancellationToken = default)
    {
        var payload = new BotControlCommandEvent(
            CommandId: command.CommandId,
            GuildId: command.GuildId.ToString(),
            UserId: command.UserId.ToString(),
            Type: command.Type,
            State: command.State.ToString(),
            ErrorMessage: command.ErrorMessage,
            ResultJson: command.ResultJson,
            CreatedAtUtc: command.CreatedAtUtc,
            ClaimedAtUtc: command.ClaimedAtUtc,
            CompletedAtUtc: command.CompletedAtUtc);

        var group = MobileUpdateGroups.UserGuild(
            command.GuildId,
            command.UserId);

        try
        {
            await hubContext.Clients
                .Group(group)
                .SendAsync(eventName, payload, cancellationToken);
            
            if (eventName != MobileRealtimeEventNames.BotControlCommandUpdated)
            {
                await hubContext.Clients
                    .Group(group)
                    .SendAsync(
                        MobileRealtimeEventNames.BotControlCommandUpdated,
                        payload,
                        cancellationToken);
            }

            logger.LogInformation(
                "Published bot control realtime event. CommandId: {CommandId}, EventName: {EventName}, GuildId: {GuildId}, UserId: {UserId}",
                command.CommandId,
                eventName,
                command.GuildId,
                command.UserId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to publish bot control realtime event. CommandId: {CommandId}, EventName: {EventName}",
                command.CommandId,
                eventName);
        }
    }
}
