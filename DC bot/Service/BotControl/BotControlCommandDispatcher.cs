using System.Text.Json;
using DC_bot.BotControl;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.BotControl;
using DC_bot.Interface.Service.BotControl.Models;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;
using DC_bot.Wrapper;
using DSharpPlus;

namespace DC_bot.Service.BotControl;

public sealed class BotControlCommandDispatcher(
    IMusicQueueService musicQueueService,
    IBotControlResultFactory botControlResultFactory,
    IBotControlContextResolver botControlContextResolver,
    ILavaLinkService lavaLinkService,
    IRepeatService repeatService,
    ICurrentTrackService currentTrackService,
    DiscordClient discordClient)
    : IBotControlCommandDispatcher
{
    public Task<BotControlCommandResult> DispatchAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken)
    {
        return command.Type switch
        {
            BotControlCommandTypes.Clear => HandleClearAsync(command, cancellationToken),
            BotControlCommandTypes.Play => HandlePlayAsync(command, cancellationToken),
            BotControlCommandTypes.Shuffle => HandleShuffleAsync(command, cancellationToken),
            BotControlCommandTypes.Remove => HandleRemoveAsync(command, cancellationToken),
            BotControlCommandTypes.MoveUp => HandleMoveAsync(command, moveUp: true, cancellationToken),
            BotControlCommandTypes.MoveDown => HandleMoveAsync(command, moveUp: false, cancellationToken),
            BotControlCommandTypes.Pause => HandlePlaybackControlAsync(
                command,
                static (service, message, member) => service.PauseAsync(message, member),
                cancellationToken),
            BotControlCommandTypes.Resume => HandlePlaybackControlAsync(
                command,
                static (service, message, member) => service.ResumeAsync(message, member),
                cancellationToken),
            BotControlCommandTypes.Skip => HandlePlaybackControlAsync(
                command,
                static (service, message, member) => service.SkipAsync(message, member),
                cancellationToken),
            BotControlCommandTypes.Previous => HandlePlaybackControlAsync(
                command,
                static (service, message, member) => service.PreviousAsync(message, member),
                cancellationToken),
            BotControlCommandTypes.Repeat => HandleRepeatAsync(command, cancellationToken),
            BotControlCommandTypes.RepeatList => HandleRepeatListAsync(command, cancellationToken),
            _ => Task.FromResult(botControlResultFactory.Failure(
                command,
                $"Unsupported bot control command type: {command.Type}",
                "UnsupportedCommandType",
                shouldNotifyDiscord: false))
        };
    }

    private async Task<BotControlCommandResult> HandleClearAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken)
    {
        var context = await botControlContextResolver.ResolveAsync(command, null, cancellationToken);

        await musicQueueService.ClearQueue(command.GuildId);

        return botControlResultFactory.Success(
            command,
            "Queue cleared.",
            voiceChannelId: context.VoiceChannelId,
            textChannelId: context.TextChannelId,
            shouldNotifyDiscord: context.CanSendDiscordResponse);
    }

    private async Task<BotControlCommandResult> HandlePlayAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken)
    {
        if (!TryDeserializePayload<QueueEnqueueCommandPayload>(command, out var payload, out var errorResult))
        {
            return errorResult!;
        }

        if (string.IsNullOrWhiteSpace(payload!.Query))
        {
            return botControlResultFactory.Failure(
                command,
                "Query is required.",
                "EmptyQuery",
                shouldNotifyDiscord: false);
        }

        var context = await botControlContextResolver.ResolveAsync(
            command,
            new BotControlCommandChannelContext(payload.VoiceChannelId, payload.TextChannelId),
            cancellationToken);

        if (context.VoiceChannelId is null)
        {
            return botControlResultFactory.Failure(
                command,
                "Join a voice channel first.",
                "UserNotInVoiceChannel",
                voiceChannelId: context.VoiceChannelId,
                textChannelId: context.TextChannelId,
                shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        _ = lavaLinkService;

        return botControlResultFactory.Failure(
            command,
            "Mobile play command is not implemented yet.",
            "PlayNotImplemented",
            new { payload.Query, payload.SearchMode },
            context.VoiceChannelId,
            context.TextChannelId,
            context.CanSendDiscordResponse);
    }

    private async Task<BotControlCommandResult> HandleShuffleAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken)
    {
        var context = await botControlContextResolver.ResolveAsync(command, null, cancellationToken);
        var shuffleResult = await musicQueueService.ShuffleQueue(command.GuildId);

        if (!shuffleResult.Success)
        {
            return botControlResultFactory.Failure(
                command,
                "Queue needs at least two tracks to shuffle.",
                "QueueTooSmall",
                new { shuffleResult.TrackCount },
                voiceChannelId: context.VoiceChannelId,
                textChannelId: context.TextChannelId,
                shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        return botControlResultFactory.Success(
            command,
            "Queue shuffled.",
            new { trackCount = shuffleResult.TrackCount },
            context.VoiceChannelId,
            context.TextChannelId,
            context.CanSendDiscordResponse);
    }

    private async Task<BotControlCommandResult> HandleRemoveAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken)
    {
        if (!TryDeserializePayload<QueueRemoveCommandPayload>(command, out var payload, out var errorResult))
        {
            return errorResult!;
        }

        var context = await botControlContextResolver.ResolveAsync(command, null, cancellationToken);
        var removeResult = await musicQueueService.RemoveAt(command.GuildId, payload!.TrackNumber);

        if (!removeResult.Success)
        {
            return botControlResultFactory.Failure(
                command,
                "Invalid track number.",
                "InvalidTrackNumber",
                new { payload.TrackNumber, queueSize = removeResult.QueueSize },
                context.VoiceChannelId,
                context.TextChannelId,
                context.CanSendDiscordResponse);
        }

        return botControlResultFactory.Success(
            command,
            "Track removed from queue.",
            new { payload.TrackNumber, Title = removeResult.RemovedTrackTitle },
            context.VoiceChannelId,
            context.TextChannelId,
            context.CanSendDiscordResponse);
    }

    private async Task<BotControlCommandResult> HandleMoveAsync(
        BotControlCommandRecord command,
        bool moveUp,
        CancellationToken cancellationToken)
    {
        if (!TryDeserializePayload<QueueMoveCommandPayload>(command, out var payload, out var errorResult))
        {
            return errorResult!;
        }

        var context = await botControlContextResolver.ResolveAsync(command, null, cancellationToken);
        var moveResult = await musicQueueService.Move(command.GuildId, payload!.TrackIndex, moveUp);

        if (!moveResult.Success)
        {
            return botControlResultFactory.Failure(
                command,
                "Track cannot be moved in that direction.",
                "InvalidTrackIndex",
                new { payload.TrackIndex, queueSize = moveResult.QueueSize },
                context.VoiceChannelId,
                context.TextChannelId,
                context.CanSendDiscordResponse);
        }

        return botControlResultFactory.Success(
            command,
            moveUp ? "Track moved up." : "Track moved down.",
            new { from = moveResult.From, to = moveResult.To },
            context.VoiceChannelId,
            context.TextChannelId,
            context.CanSendDiscordResponse);
    }

    private async Task<BotControlCommandResult> HandlePlaybackControlAsync(
        BotControlCommandRecord command,
        Func<ILavaLinkService, IDiscordMessage, IDiscordMember?, Task<PlaybackControlResult>> operation,
        CancellationToken cancellationToken)
    {
        var context = await botControlContextResolver.ResolveAsync(command, null, cancellationToken);

        if (context.VoiceChannelId is null)
        {
            return botControlResultFactory.Failure(
                command,
                "Join a voice channel first.",
                "UserNotInVoiceChannel",
                voiceChannelId: context.VoiceChannelId,
                textChannelId: context.TextChannelId,
                shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        var runtimeContext = await TryCreateRuntimeContextAsync(command, context);
        if (runtimeContext is null)
        {
            return botControlResultFactory.Failure(
                command,
                "Could not resolve Discord context for playback command.",
                "DiscordContextNotFound",
                voiceChannelId: context.VoiceChannelId,
                textChannelId: context.TextChannelId,
                shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        var controlResult = await operation(lavaLinkService, runtimeContext.Message, runtimeContext.Member);
        if (!controlResult.Success)
        {
            return botControlResultFactory.Failure(
                command,
                controlResult.Message,
                controlResult.ErrorCode ?? "PlaybackControlFailed",
                voiceChannelId: context.VoiceChannelId,
                textChannelId: context.TextChannelId,
                shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        return botControlResultFactory.Success(
            command,
            controlResult.Message,
            voiceChannelId: context.VoiceChannelId,
            textChannelId: context.TextChannelId,
            shouldNotifyDiscord: context.CanSendDiscordResponse);
    }

    private async Task<BotControlCommandResult> HandleRepeatAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken)
    {
        var context = await botControlContextResolver.ResolveAsync(command, null, cancellationToken);

        if (await repeatService.IsRepeatingListAsync(command.GuildId))
        {
            return botControlResultFactory.Failure(
                command,
                "Repeat list is already enabled.",
                "RepeatListAlreadyEnabled",
                voiceChannelId: context.VoiceChannelId,
                textChannelId: context.TextChannelId,
                shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        if (await repeatService.IsRepeatingAsync(command.GuildId))
        {
            await repeatService.SetRepeatingAsync(command.GuildId, false);
            return botControlResultFactory.Success(
                command,
                "Repeat disabled.",
                voiceChannelId: context.VoiceChannelId,
                textChannelId: context.TextChannelId,
                shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        await repeatService.SetRepeatingAsync(command.GuildId, true);
        var currentTrack = await currentTrackService.GetCurrentTrackAsync(command.GuildId);

        return botControlResultFactory.Success(
            command,
            "Repeat enabled.",
            new { trackTitle = currentTrack?.Title },
            context.VoiceChannelId,
            context.TextChannelId,
            context.CanSendDiscordResponse);
    }

    private async Task<BotControlCommandResult> HandleRepeatListAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken)
    {
        var context = await botControlContextResolver.ResolveAsync(command, null, cancellationToken);

        if (await repeatService.IsRepeatingAsync(command.GuildId))
        {
            return botControlResultFactory.Failure(
                command,
                "Single-track repeat is already enabled.",
                "RepeatAlreadyEnabled",
                voiceChannelId: context.VoiceChannelId,
                textChannelId: context.TextChannelId,
                shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        if (await repeatService.IsRepeatingListAsync(command.GuildId))
        {
            await repeatService.SetRepeatingListAsync(command.GuildId, false);
            return botControlResultFactory.Success(
                command,
                "Repeat list disabled.",
                voiceChannelId: context.VoiceChannelId,
                textChannelId: context.TextChannelId,
                shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        var queue = await musicQueueService.ViewQueue(command.GuildId);
        var currentTrack = await currentTrackService.GetCurrentTrackAsync(command.GuildId);

        await repeatService.SaveRepeatListSnapshotAsync(command.GuildId, currentTrack, queue);
        await repeatService.SetRepeatingListAsync(command.GuildId, true);

        return botControlResultFactory.Success(
            command,
            "Repeat list enabled.",
            new { currentTrackTitle = currentTrack?.Title, queuedTrackCount = queue.Count },
            context.VoiceChannelId,
            context.TextChannelId,
            context.CanSendDiscordResponse);
    }

    private async Task<BotControlRuntimeContext?> TryCreateRuntimeContextAsync(
        BotControlCommandRecord command,
        BotControlExecutionContext context)
    {
        if (context.TextChannelId is null)
        {
            return null;
        }

        var guild = await BotControlDiscordChannelResolver.TryGetGuildAsync(discordClient, command.GuildId);
        if (guild is null)
        {
            return null;
        }

        var channel = await BotControlDiscordChannelResolver.TryGetGuildChannelAsync(guild, context.TextChannelId.Value);
        if (channel is null || !BotControlDiscordChannelResolver.IsMessageCapableChannel(channel))
        {
            return null;
        }

        var member = guild.Members.TryGetValue(command.UserId, out var cachedMember)
            ? cachedMember
            : await guild.GetMemberAsync(command.UserId);

        var wrappedChannel = new DiscordChannelWrapper(channel, guild: guild);
        var wrappedMember = new BotControlDiscordMember(member, wrappedChannel);
        var message = new BotControlDiscordMessage(wrappedChannel, new DiscordUserWrapper(member));

        return new BotControlRuntimeContext(message, wrappedMember);
    }

    private sealed record BotControlRuntimeContext(
        IDiscordMessage Message,
        IDiscordMember Member);

    private bool TryDeserializePayload<T>(
        BotControlCommandRecord command,
        out T? payload,
        out BotControlCommandResult? errorResult)
    {
        payload = default;
        errorResult = null;

        if (string.IsNullOrWhiteSpace(command.PayloadJson))
        {
            errorResult = botControlResultFactory.Failure(
                command,
                "Command payload is required.",
                "InvalidPayload",
                shouldNotifyDiscord: false);
            return false;
        }

        try
        {
            payload = JsonSerializer.Deserialize<T>(command.PayloadJson);
        }
        catch (JsonException)
        {
            payload = default;
        }

        if (payload is not null)
        {
            return true;
        }

        errorResult = botControlResultFactory.Failure(
            command,
            "Command payload is invalid.",
            "InvalidPayload",
            shouldNotifyDiscord: false);
        return false;
    }
}
