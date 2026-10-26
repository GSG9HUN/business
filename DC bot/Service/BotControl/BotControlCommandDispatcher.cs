using System.Text.Json;
using DC_bot.BotControl;
using DC_bot.BotControl.Payloads;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.BotControl;
using DC_bot.Interface.Service.BotControl.Models;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Music.Queue;
using DC_bot.Interface.Service.Music.Repeat;
using DC_bot.Interface.Service.Music.Search;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;
using DC_bot.Wrapper.Discord;
using DSharpPlus;

namespace DC_bot.Service.BotControl;

/// <summary>
/// Dispatches durable mobile commands to playback services and produces structured outcomes.
/// </summary>
/// <param name="musicQueueService">The music queue service dependency used by this boundary.</param>
/// <param name="botControlResultFactory">The bot control result factory dependency used by this boundary.</param>
/// <param name="botControlContextResolver">The bot control context resolver dependency used by this boundary.</param>
/// <param name="lavaLinkService">The lava link service dependency used by this boundary.</param>
/// <param name="repeatService">The repeat service dependency used by this boundary.</param>
/// <param name="currentTrackService">The current track service dependency used by this boundary.</param>
/// <param name="discordClient">The discord client supplied for this operation.</param>
/// <param name="trackSearchResolverService">The track search resolver service dependency used by this boundary.</param>
public sealed class BotControlCommandDispatcher(
    IMusicQueueService musicQueueService,
    IBotControlResultFactory botControlResultFactory,
    IBotControlContextResolver botControlContextResolver,
    ILavaLinkService lavaLinkService,
    IRepeatService repeatService,
    ICurrentTrackService currentTrackService,
    DiscordClient discordClient,
    ITrackSearchResolverService trackSearchResolverService) : IBotControlCommandDispatcher
{

    /// <summary>
    /// Dispatches the  operation to its resolved service handler.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<BotControlCommandResult> DispatchAsync(BotControlCommandRecord command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return command.Type switch
        {
            BotControlCommandTypes.Clear => HandleClearAsync(command, cancellationToken),
            BotControlCommandTypes.Play => HandlePlayAsync(command, cancellationToken),
            BotControlCommandTypes.Shuffle => HandleShuffleAsync(command, cancellationToken),
            BotControlCommandTypes.Remove => HandleRemoveAsync(command, cancellationToken),
            BotControlCommandTypes.MoveToIndex => HandleMoveToIndexAsync(command, cancellationToken),
            BotControlCommandTypes.Pause => HandlePlaybackControlAsync(command, (service, message, member) => service.PauseAsync(message, member, cancellationToken), cancellationToken),
            BotControlCommandTypes.Resume => HandlePlaybackControlAsync(command, (service, message, member) => service.ResumeAsync(message, member, cancellationToken), cancellationToken),
            BotControlCommandTypes.Skip => HandlePlaybackControlAsync(command, (service, message, member) => service.SkipAsync(message, member, cancellationToken), cancellationToken),
            BotControlCommandTypes.Previous => HandlePlaybackControlAsync(command, (service, message, member) => service.PreviousAsync(message, member, cancellationToken), cancellationToken),
            BotControlCommandTypes.Repeat => HandleRepeatAsync(command, cancellationToken),
            BotControlCommandTypes.RepeatList => HandleRepeatListAsync(command, cancellationToken),
            _ => Task.FromResult(botControlResultFactory.Failure(command, $"Unsupported bot control command type: {command.Type}", "UnsupportedCommandType", shouldNotifyDiscord: false))};
    }

    /// <summary>
    /// Routes the clear operation through its resolved context and records the outcome.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<BotControlCommandResult> HandleClearAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = await botControlContextResolver.ResolveAsync(command, null, cancellationToken);
        await musicQueueService.ClearQueue(command.GuildId, cancellationToken);
        return botControlResultFactory.Success(command, "Queue cleared.", voiceChannelId: context.VoiceChannelId, textChannelId: context.TextChannelId, shouldNotifyDiscord: context.CanSendDiscordResponse);
    }

    /// <summary>
    /// Routes the play operation through its resolved context and records the outcome.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<BotControlCommandResult> HandlePlayAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (
            !TryDeserializePayload<QueueEnqueueCommandPayload>(
            command,
            out var payload,
            out var errorResult))
        {
            return errorResult!;
        }

        if (string.IsNullOrWhiteSpace(payload!.Query))
        {
            return botControlResultFactory.Failure(command, "Query is required.", "EmptyQuery", shouldNotifyDiscord: false);
        }

        if (
            !MobilePlayInput.TryParse(
            payload.Query,
            payload.SearchMode,
            trackSearchResolverService,
            out var input))
        {
            return botControlResultFactory.Failure(command, "Invalid music query, provider URL or search mode.", "InvalidPlayInput", shouldNotifyDiscord: false);
        }

        var context = await botControlContextResolver.ResolveAsync(command, new BotControlCommandChannelContext(payload.VoiceChannelId, payload.TextChannelId), cancellationToken);

        if (context.VoiceChannelId is null)
        {
            return botControlResultFactory.Failure(command, "Join a voice channel first.", "UserNotInVoiceChannel", voiceChannelId: context.VoiceChannelId, textChannelId: context.TextChannelId, shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        var runtime = await TryCreateRuntimeContextAsync(command, context, cancellationToken);

        if (runtime?.Member.VoiceState?.Channel is not { } voiceChannel)
        {
            return botControlResultFactory.Failure(command, "Could not resolve Discord context for playback.", "DiscordContextNotFound", voiceChannelId: context.VoiceChannelId, textChannelId: context.TextChannelId, shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        cancellationToken.ThrowIfCancellationRequested();
        PlaybackStartResult result;
        try
        {
            result = input!.Url is { } url ? await lavaLinkService.PlayAsyncUrl(voiceChannel, url, runtime.Message, input.SearchMode, payload.RequestedBy, cancellationToken) : await lavaLinkService.PlayAsyncQuery(voiceChannel, input.Query, runtime.Message, input.SearchMode, payload.RequestedBy, cancellationToken);
        }
        catch (TrackLoadException)
        {
            return botControlResultFactory.Failure(command, "Could not load the selected music.", "TrackLoadFailed", voiceChannelId: context.VoiceChannelId, textChannelId: context.TextChannelId, shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        var data = new
        {
            playbackStatus = result.Status.ToString(),
            operationId = result.OperationId,
            queueItemId = result.QueueItemId,
            requiresReconciliation = result.Status is PlaybackStartStatus.Unknown or PlaybackStartStatus.StartedStatePending or PlaybackStartStatus.Busy
        };
        var message = result.Status switch
        {
            PlaybackStartStatus.Started => "Playback started.",
            PlaybackStartStatus.Queued => "Music added to playback queue.",
            PlaybackStartStatus.StartedStatePending => "Playback started; saving its state is not confirmed. Do not resubmit.",
            PlaybackStartStatus.Unknown => "Request submitted; playback outcome is not confirmed. Do not resubmit.",
            PlaybackStartStatus.Busy => "A previous playback operation needs reconciliation; no new playback was started.",
            PlaybackStartStatus.Superseded => "A newer session or leave operation superseded this request.",
            PlaybackStartStatus.Empty => "No queued track is available.",
            _ => "Playback did not start."
        };

        if (
            result.Status is
            PlaybackStartStatus.Started or PlaybackStartStatus.Queued or PlaybackStartStatus.StartedStatePending or PlaybackStartStatus.Unknown)
        {
            return botControlResultFactory.Success(command, message, data, context.VoiceChannelId, context.TextChannelId, context.CanSendDiscordResponse);
        }

        return botControlResultFactory.Failure(command, message, result.Status.ToString(), data, context.VoiceChannelId, context.TextChannelId, context.CanSendDiscordResponse);
    }

    /// <summary>
    /// Routes the shuffle operation through its resolved context and records the outcome.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<BotControlCommandResult> HandleShuffleAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = await botControlContextResolver.ResolveAsync(command, null, cancellationToken);
        var shuffleResult = await musicQueueService.ShuffleQueue(command.GuildId, cancellationToken);

        if (!shuffleResult.Success)
        {
            return botControlResultFactory.Failure(command, "Queue needs at least two tracks to shuffle.", "QueueTooSmall", new { shuffleResult.TrackCount }, voiceChannelId: context.VoiceChannelId, textChannelId: context.TextChannelId, shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        return botControlResultFactory.Success(command, "Queue shuffled.", new { trackCount = shuffleResult.TrackCount }, context.VoiceChannelId, context.TextChannelId, context.CanSendDiscordResponse);
    }

    /// <summary>
    /// Routes the remove operation through its resolved context and records the outcome.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<BotControlCommandResult> HandleRemoveAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (
            !TryDeserializePayload<QueueRemoveCommandPayload>(
            command,
            out var payload,
            out var errorResult))
        {
            return errorResult!;
        }

        var context = await botControlContextResolver.ResolveAsync(command, null, cancellationToken);
        var removeResult = await musicQueueService.RemoveAt(command.GuildId, payload!.TrackNumber, cancellationToken);

        if (!removeResult.Success)
        {
            return botControlResultFactory.Failure(command, "Invalid track number.", "InvalidTrackNumber", new { payload.TrackNumber, queueSize = removeResult.QueueSize }, context.VoiceChannelId, context.TextChannelId, context.CanSendDiscordResponse);
        }

        return botControlResultFactory.Success(command, "Track removed from queue.", new { payload.TrackNumber, Title = removeResult.RemovedTrackTitle }, context.VoiceChannelId, context.TextChannelId, context.CanSendDiscordResponse);
    }

    /// <summary>
    /// Routes the move to index operation through its resolved context and records the outcome.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<BotControlCommandResult> HandleMoveToIndexAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (
            !TryDeserializePayload<QueueMoveToIndexCommandPayload>(
            command,
            out var payload,
            out var errorResult))
        {
            return errorResult!;
        }

        var context = await botControlContextResolver.ResolveAsync(command, null, cancellationToken);
        var moveResult = await musicQueueService.MoveToIndex(command.GuildId, payload!.TrackIndex, payload.TargetIndex, cancellationToken);

        if (!moveResult.Success)
        {
            return botControlResultFactory.Failure(command, "Track cannot be moved to that position.", "InvalidTrackIndex", new { payload.TrackIndex, payload.TargetIndex, queueSize = moveResult.QueueSize }, context.VoiceChannelId, context.TextChannelId, context.CanSendDiscordResponse);
        }

        return botControlResultFactory.Success(command, "Track moved.", new { from = moveResult.From, to = moveResult.To }, context.VoiceChannelId, context.TextChannelId, context.CanSendDiscordResponse);
    }

    /// <summary>
    /// Routes the playback control operation through its resolved context and records the outcome.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="operation">The operation supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<BotControlCommandResult> HandlePlaybackControlAsync(
        BotControlCommandRecord command,
        Func<ILavaLinkService, IDiscordMessage, IDiscordMember?, Task<PlaybackControlResult>> operation,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = await botControlContextResolver.ResolveAsync(command, null, cancellationToken);

        if (context.VoiceChannelId is null)
        {
            return botControlResultFactory.Failure(command, "Join a voice channel first.", "UserNotInVoiceChannel", voiceChannelId: context.VoiceChannelId, textChannelId: context.TextChannelId, shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        var runtimeContext = await TryCreateRuntimeContextAsync(command, context, cancellationToken);

        if (runtimeContext is null)
        {
            return botControlResultFactory.Failure(command, "Could not resolve Discord context for playback command.", "DiscordContextNotFound", voiceChannelId: context.VoiceChannelId, textChannelId: context.TextChannelId, shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        var controlResult = await operation(lavaLinkService, runtimeContext.Message, runtimeContext.Member);

        if (!controlResult.Success)
        {
            return botControlResultFactory.Failure(command, controlResult.Message, controlResult.ErrorCode ?? "PlaybackControlFailed", voiceChannelId: context.VoiceChannelId, textChannelId: context.TextChannelId, shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        return botControlResultFactory.Success(command, controlResult.Message, voiceChannelId: context.VoiceChannelId, textChannelId: context.TextChannelId, shouldNotifyDiscord: context.CanSendDiscordResponse);
    }

    /// <summary>
    /// Routes the repeat operation through its resolved context and records the outcome.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<BotControlCommandResult> HandleRepeatAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = await botControlContextResolver.ResolveAsync(command, null, cancellationToken);

        if (
            await repeatService.IsRepeatingListAsync(
            command.GuildId,
            cancellationToken))
        {
            return botControlResultFactory.Failure(command, "Repeat list is already enabled.", "RepeatListAlreadyEnabled", voiceChannelId: context.VoiceChannelId, textChannelId: context.TextChannelId, shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        if (
            await repeatService.IsRepeatingAsync(
            command.GuildId,
            cancellationToken))
        {
            await repeatService.SetRepeatingAsync(command.GuildId, false, cancellationToken);
            return botControlResultFactory.Success(command, "Repeat disabled.", voiceChannelId: context.VoiceChannelId, textChannelId: context.TextChannelId, shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        await repeatService.SetRepeatingAsync(command.GuildId, true, cancellationToken);
        var currentTrack = await currentTrackService.GetCurrentTrackAsync(command.GuildId, cancellationToken);
        return botControlResultFactory.Success(command, "Repeat enabled.", new { trackTitle = currentTrack?.Title }, context.VoiceChannelId, context.TextChannelId, context.CanSendDiscordResponse);
    }

    /// <summary>
    /// Routes the repeat list operation through its resolved context and records the outcome.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<BotControlCommandResult> HandleRepeatListAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = await botControlContextResolver.ResolveAsync(command, null, cancellationToken);

        if (
            await repeatService.IsRepeatingAsync(
            command.GuildId,
            cancellationToken))
        {
            return botControlResultFactory.Failure(command, "Single-track repeat is already enabled.", "RepeatAlreadyEnabled", voiceChannelId: context.VoiceChannelId, textChannelId: context.TextChannelId, shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        if (
            await repeatService.IsRepeatingListAsync(
            command.GuildId,
            cancellationToken))
        {
            await repeatService.SetRepeatingListAsync(command.GuildId, false, cancellationToken);
            return botControlResultFactory.Success(command, "Repeat list disabled.", voiceChannelId: context.VoiceChannelId, textChannelId: context.TextChannelId, shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        if (
            !await musicQueueService.TryEnableBatchRepeatAsync(
            command.GuildId,
            cancellationToken))
        {
            return botControlResultFactory.Failure(command, "There is no active playback batch to repeat.", "NoActivePlaybackBatch", voiceChannelId: context.VoiceChannelId, textChannelId: context.TextChannelId, shouldNotifyDiscord: context.CanSendDiscordResponse);
        }

        var queue = await musicQueueService.GetRepeatableQueue(command.GuildId, cancellationToken);
        var currentTrack = await currentTrackService.GetCurrentTrackAsync(command.GuildId, cancellationToken);
        return botControlResultFactory.Success(command, "Repeat list enabled.", new { currentTrackTitle = currentTrack?.Title, queuedTrackCount = (await musicQueueService.ViewQueue(command.GuildId, cancellationToken)).Count, batchTrackCount = queue.Count }, context.VoiceChannelId, context.TextChannelId, context.CanSendDiscordResponse);
    }

    /// <summary>
    /// Resolves Discord adapters needed to execute a mobile command in its selected channels.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="context">The context supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<BotControlRuntimeContext?> TryCreateRuntimeContextAsync(
        BotControlCommandRecord command,
        BotControlExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.TextChannelId is null)
        {
            return null;
        }

        var guild = await BotControlDiscordChannelResolver.TryGetGuildAsync(discordClient, command.GuildId, cancellationToken);

        if (guild is null)
        {
            return null;
        }

        var channel = await BotControlDiscordChannelResolver.TryGetGuildChannelAsync(guild, context.TextChannelId.Value, cancellationToken);

        if (channel is null
            || !BotControlDiscordChannelResolver.IsMessageCapableChannel(channel))
        {
            return null;
        }

        var member = guild.Members.TryGetValue(command.UserId, out var cachedMember) ? cachedMember : await guild.GetMemberAsync(command.UserId)
            .WaitAsync(cancellationToken);

        if (context.VoiceChannelId is null)
        {
            return null;
        }

        var voiceChannel = await BotControlDiscordChannelResolver.TryGetGuildChannelAsync(guild, context.VoiceChannelId.Value, cancellationToken);

        if (!BotControlDiscordChannelResolver.IsVoiceCapableChannel(voiceChannel))
        {
            return null;
        }

        var wrappedChannel = new DiscordChannelWrapper(channel, guild: guild);
        var wrappedMember = new BotControlDiscordMember(member, new DiscordChannelWrapper(voiceChannel!, guild: guild));
        var message = new BotControlDiscordMessage(wrappedChannel, new DiscordUserWrapper(member));
        return new BotControlRuntimeContext(message, wrappedMember);
    }

    /// <summary>
    /// Deserializes a payload and builds a structured failure for missing or invalid data.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="payload">The payload supplied for this operation.</param>
    /// <param name="errorResult">The error result supplied for this operation.</param>
    /// <typeparam name = "T">The type of value consumed or produced by the operation.</typeparam>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    private bool TryDeserializePayload<T>(BotControlCommandRecord command, out T? payload, out BotControlCommandResult? errorResult)
    {
        payload = default;
        errorResult = null;

        if (string.IsNullOrWhiteSpace(command.PayloadJson))
        {
            errorResult = botControlResultFactory.Failure(command, "Command payload is required.", "InvalidPayload", shouldNotifyDiscord: false);
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

        errorResult = botControlResultFactory.Failure(command, "Command payload is invalid.", "InvalidPayload", shouldNotifyDiscord: false);
        return false;
    }
}
