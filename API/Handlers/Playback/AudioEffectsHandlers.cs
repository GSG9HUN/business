using System.Text.Json;
using API.Mapping;
using API.Requests.Playback;
using DC_bot.BotControl;
using DC_bot.BotControl.Payloads;
using DC_bot.Interface.Service.Persistence.BotControl;
using DC_bot.Interface.Service.Persistence.Filters;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Playback;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Handlers.Playback;

/// <summary>Authorizes guild-local filter inspection and durable filter/preset commands.</summary>
public static class AudioEffectsHandlers
{

    /// <summary>Returns the last confirmed filter map and one page of guild presets; null filters require synchronization.</summary>
    public static async Task<IResult> GetAsync(
        HttpContext httpContext,
        IPlaybackStateRepository playbackRepository,
        IFilterPresetRepository presetRepository,
        IMobileAppUserRepository userRepository,
        CancellationToken ct,
        int page = 1)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var (userId, accessError) = await ApiUserContext.RequireGuildAccessAsync(httpContext, userRepository, guildId, ct);

        if (accessError is not null)
        {
            return accessError;
        }

        var state = await playbackRepository.GetOrCreateAsync(guildId, ct);
        var count = await presetRepository.CountAsync(guildId, ct);
        var pages = Math.Max(1, (int)Math.Ceiling(count / 10d));
        page = Math.Clamp(page, 1, pages);
        var presets = await presetRepository.ListAsync(guildId, page, ct);
        var guild = await userRepository.GetGuildForUserAsync(userId, guildId, ct);
        var canManage = guild is not null
            && (guild.IsOwner || (guild.Permissions & (8UL | 32UL)) != 0);
        JsonElement? filters = state.AudioEffectsJson is null ? null : JsonSerializer.Deserialize<JsonElement>(state.AudioEffectsJson);
        return HttpResults.Ok(new
        {
            Filters = filters,
            Presets = presets.Select(preset => preset.Name),
            CanManagePresets = canManage,
            Page = page,
            Pages = pages
        });
    }

    /// <summary>Enqueues effect operations; preset saves must use the dedicated preset endpoint.</summary>
    public static async Task<IResult> ApplyAsync(
        HttpContext httpContext,
        AudioEffectsCommandPayload request,
        IBotControlCommandsRepository commandRepository,
        IMobileAppUserRepository userRepository,
        CancellationToken ct)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var (userId, accessError) = await ApiUserContext.RequireGuildAccessAsync(httpContext, userRepository, guildId, ct);

        if (accessError is not null)
        {
            return accessError;
        }

        if (!request.IsWellFormed()
            || request.Commands.Any(IsPresetSaveCommand))
        {
            return HttpResults.BadRequest(new { ErrorCode = "InvalidAudioEffectsRequest" });
        }

        var command = await commandRepository.EnqueueAsync(
            guildId, userId, BotControlCommandTypes.AudioEffects, JsonSerializer.Serialize(request), ct);
        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    /// <summary>Enqueues a preset-only save; the bot persists the draft without updating live player filters.</summary>
    /// <param name="httpContext">Authenticated request and validated guild identifier.</param>
    /// <param name="request">Preset identity, overwrite consent and optional draft setting operations.</param>
    /// <param name="commandRepository">Durable command storage used for asynchronous execution.</param>
    /// <param name="userRepository">Stored user-to-guild access checks.</param>
    /// <param name="ct">Cancels authorization and enqueueing.</param>
    /// <returns>Accepted command identity, or an authorization or validation error.</returns>
    public static async Task<IResult> SavePresetAsync(
        HttpContext httpContext,
        SaveAudioEffectPresetRequest request,
        IBotControlCommandsRepository commandRepository,
        IMobileAppUserRepository userRepository,
        CancellationToken ct)
    {
        var guildId = (ulong)httpContext.Items["guildId"]!;
        var (userId, accessError) = await ApiUserContext.RequireGuildAccessAsync(httpContext, userRepository, guildId, ct);

        if (accessError is not null)
        {
            return accessError;
        }

        if (request.Name is not { Length: >= 1 and <= 32 }
            || request.Name.Any(character => !char.IsAsciiLetterOrDigit(character)
                && character is not '_' and not '-')
            || request.Commands is not { Length: <= 39 }
            || request.Commands.Any(command => !IsDraftSettingCommand(command)))
        {
            return HttpResults.BadRequest(new { ErrorCode = "InvalidAudioEffectPresetRequest" });
        }

        var saveCommand = $"custom save {request.Name}" + (request.Overwrite ? " overwrite" : "");
        var payload = new AudioEffectsCommandPayload([.. request.Commands, saveCommand]);
        var command = await commandRepository.EnqueueAsync(
            guildId, userId, BotControlCommandTypes.AudioEffects, JsonSerializer.Serialize(payload), ct);
        return BotControlCommandHttpMapper.ToAccepted(command);
    }

    /// <summary>Detects preset-save operations even when their words use additional spaces or different casing.</summary>
    private static bool IsPresetSaveCommand(string command)
    {
        var arguments = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return arguments.Length >= 2
            && arguments[0].Equals("custom", StringComparison.OrdinalIgnoreCase)
            && arguments[1].Equals("save", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Limits a save request to bounded setting edits; player actions and nested preset operations are rejected.</summary>
    private static bool IsDraftSettingCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command)
            || command.Length > 256
            || command.Any(char.IsControl))
        {
            return false;
        }

        var arguments = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return arguments[0] is "speed" or "pitch" or "advanced";
    }
}
