using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Text.Json;
using DC_bot.BotControl;
using DC_bot.Constants;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Persistence.Filters;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Presentation.Models;
using DC_bot.Service.Music.Lifecycle.Models;
using DC_bot.Service.Music.Lifecycle.Sessions;
using DC_bot.Service.Music.Playback.Models;
using DSharpPlus.Entities;
using Lavalink4NET.Filters;
using Lavalink4NET.Players;
using Lavalink4NET.Protocol.Models.Filters;
using Lavalink4NET.Rest;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Playback;

/// <summary>Composes complete guild filters under existing playback ownership and confirms only acknowledged or observed state.</summary>
/// <param name="sessions">Owns player generations, control reservations, transport draining, and filter snapshots.</param>
/// <param name="validationService">Validates the existing player connection without creating another player.</param>
/// <param name="playbackStateRepository">Refreshes the source position anchor after timescale changes.</param>
/// <param name="localizationService">Resolves English and Hungarian command messages.</param>
/// <param name="presetRepository">Persists versioned guild-local custom filter definitions.</param>
/// <param name="logger">Records uncertain transport, reconciliation, and recovery failures.</param>
public sealed class PlaybackFilterService(
    GuildPlaybackSessions sessions,
    IValidationService validationService,
    IPlaybackStateRepository playbackStateRepository,
    ILocalizationService localizationService,
    ILogger<PlaybackFilterService> logger,
    IFilterPresetRepository presetRepository) : IPlaybackFilterService
{
    private static readonly string[] PresetNames =
        ["flat", "bass-light", "bass-heavy", "nightcore", "slowed", "karaoke", "rotation", "mono", "soft"];
    private readonly ConcurrentDictionary<ILavalinkApiClient,
        (string SessionId, string Version, DateTimeOffset Expires, ImmutableHashSet<string> Filters)> _capabilities = new();

    /// <summary>Preserves plugin filter values when the SDK commits the complete built-in filter map.</summary>
    private sealed class PluginFilterOptions(IDictionary<string, JsonElement> filters) : IFilterOptions
    {

        /// <inheritdoc />
        public bool IsDefault => filters.Count == 0;

        /// <inheritdoc />
        public void Apply(ref PlayerFilterMapModel model)
        {
            model = model with { AdditionalFilters = filters };
        }
    }

    /// <inheritdoc />
    public Task<PlaybackControlResult> FiltersAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string? arguments,
        CancellationToken cancellationToken = default)
    {
        var parts = (arguments ?? "show").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var operation = parts.Length == 0 ? "show" : parts[0].ToLowerInvariant();

        if (operation == "custom")
        {
            return CustomAsync(message, member, parts.Skip(1)
                .ToArray(), cancellationToken);
        }

        if (operation == "advanced")
        {
            if (parts.Length == 1
                || (parts.Length == 2 && parts[1].Equals("help", StringComparison.OrdinalIgnoreCase)))
            {
                return Task.FromResult(PlaybackControlResult.Information(
                    localizationService.Get(message.Channel.Guild.Id, LocalizationKeys.FiltersAdvancedHelp)));
            }

            var advanced = parts.Skip(1)
                .ToArray();

            if (!TryCreateAdvancedMutation(advanced, out var mutation))
            {
                return Task.FromResult(Reject(message.Channel.Guild.Id, LocalizationKeys.FiltersAdvancedInvalid));
            }

            return ExecuteAsync(message, member, "advanced", mutation, null, cancellationToken);
        }

        if (parts.Length <= 1 && operation is "show" or "list")
        {
            return ExecuteAsync(message, member, operation, null, null, cancellationToken);
        }

        if (parts.Length == 1 && operation == "reset")
        {
            return ExecuteAsync(message, member, operation, _ => new PlaybackFilterState(new()), null, cancellationToken);
        }

        if (parts.Length == 2 && operation == "preset")
        {
            var name = parts[1].ToLowerInvariant();
            var preset = CreatePreset(name);

            if (preset is not null)
            {
                return ExecuteAsync(message, member, operation, _ => preset, name, cancellationToken);
            }
        }

        return Task.FromResult(Reject(message.Channel.Guild.Id, LocalizationKeys.FiltersCommandInvalid));
    }

    /// <inheritdoc />
    public Task<PlaybackControlResult> BassBoostAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string? level,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(level))
        {
            return ExecuteAsync(message, member, "show", null, null, cancellationToken);
        }

        var gain = level.Trim().ToLowerInvariant() switch
        {
            "off" => 0f,
            "low" => 0.06f,
            "medium" => 0.12f,
            "high" => 0.20f,
            _ => -1f
        };

        if (gain < 0)
        {
            return Task.FromResult(Reject(message.Channel.Guild.Id, LocalizationKeys.BassBoostCommandInvalid));
        }

        return ExecuteAsync(message, member, "bassboost", current =>
            new PlaybackFilterState(current.Filters with { Equalizer = BassBands(gain) }), null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PlaybackControlResult> SpeedAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string? multiplier,
        CancellationToken cancellationToken = default)
    {
        return SetTimescaleAsync(message, member, "speed", multiplier, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PlaybackControlResult> PitchAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string? multiplier,
        CancellationToken cancellationToken = default)
    {
        return SetTimescaleAsync(message, member, "pitch", multiplier, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RestoreAsync(ILavalinkPlayer player, CancellationToken cancellationToken = default)
    {
        var confirmed = sessions.GetConfirmedFilters(player.GuildId);

        if (confirmed is null)
        {
            return;
        }

        var supported = await GetSupportedFiltersAsync(player, cancellationToken);

        if (!IsSupported(confirmed, supported))
        {
            throw new InvalidOperationException("The replacement Lavalink node does not support the confirmed filters.");
        }

        ApplyToSdk(player.Filters, confirmed);
        await player.Filters.CommitAsync(cancellationToken);
        sessions.ConfirmRecoveredFilters(player, confirmed);
    }

    /// <summary>Parses an invariant speed or pitch and modifies only that timescale component.</summary>
    private Task<PlaybackControlResult> SetTimescaleAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string operation,
        string? multiplier,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(multiplier))
        {
            return ExecuteAsync(message, member, operation, null, null, cancellationToken);
        }

        if (!double.TryParse(multiplier.Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed)
            || !double.IsFinite(parsed)
            || parsed < 0.5
            || parsed > 2)
        {
            return Task.FromResult(Reject(message.Channel.Guild.Id, LocalizationKeys.FiltersParameterInvalid));
        }

        var value = (float)parsed;
        return ExecuteAsync(message, member, operation, current =>
        {
            var timescale = current.Filters.Timescale ?? new TimescaleFilterModel(1f, 1f, 1f);
            timescale = operation == "speed" ? timescale with { Speed = value } : timescale with { Pitch = value };
            return new PlaybackFilterState(current.Filters with { Timescale = timescale });
        }, null, cancellationToken);
    }

    /// <summary>Validates, reads, composes, and commits one complete configuration under the guild control reservation.</summary>
    private async Task<PlaybackControlResult> ExecuteAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string operation,
        Func<PlaybackFilterState, PlaybackFilterState>? mutation,
        string? presetName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var guildId = message.Channel.Guild.Id;
        var session = sessions.CaptureCurrent(guildId);

        if (session is null)
        {
            return Reject(guildId, ValidationErrorKeys.BotIsNotConnectedError);
        }

        if (mutation is not null
            && (member?.VoiceState?.Channel is not { } voice
                || voice.Guild.Id != guildId
                || voice.Id != session.Player.VoiceChannelId))
        {
            return Reject(guildId, LocalizationKeys.PlaybackWrongVoiceChannel);
        }

        ControlReservation? reservation = null;
        PlaybackFilterState? previous = null;
        var dispatched = false;
        var committed = false;
        try
        {
            reservation = sessions.ReserveControl(session, "Filters." + operation);
            var connection = await validationService.ValidateConnectionAsync(session.Player, cancellationToken);
            sessions.ValidateControl(session, reservation);

            if (!connection.IsValid)
            {
                return Reject(guildId, ValidationErrorKeys.BotIsNotConnectedError);
            }

            if (operation == "list")
            {
                var supported = await GetSupportedFiltersAsync(session.Player, cancellationToken);
                sessions.ValidateControl(session, reservation);
                var names = PresetNames.Where(name => IsSupported(CreatePreset(name)!, supported));
                return PlaybackControlResult.Information(localizationService.Get(
                    guildId, LocalizationKeys.FiltersCommandPresets, string.Join(", ", names)));
            }

            previous = await ReadRemoteAsync(session, reservation, cancellationToken);

            if (mutation is null)
            {
                var result = operation switch
                {
                    "speed" => localizationService.Get(guildId, LocalizationKeys.SpeedCommandCurrent,
                        Number(previous.Filters.Timescale?.Speed ?? 1f)),
                    "pitch" => localizationService.Get(guildId, LocalizationKeys.PitchCommandCurrent,
                        Number(previous.Filters.Timescale?.Pitch ?? 1f)),
                    _ => FormatState(guildId, previous, session.Player.Volume)
                };
                return PlaybackControlResult.Information(result);
            }

            var desired = Snapshot(mutation(previous).Filters);
            var capabilities = await GetSupportedFiltersAsync(session.Player, cancellationToken);
            sessions.ValidateControl(session, reservation);

            if (!IsSupported(desired, capabilities))
            {
                return Reject(guildId, LocalizationKeys.FiltersCommandUnsupported);
            }

            var queueItemId = sessions.GetReadyQueueItemId(session);
            var oldTimescale = previous.Filters.Timescale;
            sessions.BeginFilterChange(session, desired);
            ApplyToSdk(session.Player.Filters, desired);
            await sessions.RunTransportAsync(session, async () =>
            {
                sessions.ValidateControl(session, reservation);
                cancellationToken.ThrowIfCancellationRequested();
                dispatched = true;
                await session.Player.Filters.CommitAsync(cancellationToken);
                return true;
            });
            sessions.ValidateControl(session, reservation);
            committed = true;
            sessions.ConfirmFilters(session, desired);
            var warning = operation == "preset" && presetName == "bass-heavy"
                ? localizationService.Get(guildId, LocalizationKeys.FiltersCommandHeavyWarning)
                : string.Empty;

            if (oldTimescale != desired.Filters.Timescale
                && !await RefreshPositionAsync(session, reservation, queueItemId, cancellationToken))
            {
                warning += localizationService.Get(guildId, LocalizationKeys.FiltersCommandPositionPending);
            }

            var summary = FormatState(guildId, desired, session.Player.Volume);
            var text = operation switch
            {
                "reset" => localizationService.Get(guildId, LocalizationKeys.FiltersCommandReset,
                    Number(session.Player.Volume * 100f)) + warning,
                "preset" or "custom-preset" => localizationService.Get(guildId, LocalizationKeys.FiltersCommandPresetApplied,
                    presetName!, summary, warning),
                _ => localizationService.Get(guildId, LocalizationKeys.FiltersCommandApplied, summary, warning)
            };
            return warning.Length > 0 ? PlaybackControlResult.Warning(text) : PlaybackControlResult.Succeeded(text);
        }
        catch (PlaybackOperationBusyException)
        {
            return Reject(guildId, ValidationErrorKeys.PlaybackBusy);
        }
        catch (PlaybackSessionExpiredException)
        {
            return Reject(guildId, ValidationErrorKeys.PlaybackSuperseded);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            if (!committed
                && previous is not null
                && sessions.IsCurrent(session))
            {
                ApplyToSdk(session.Player.Filters, previous);

                if (!dispatched)
                {
                    sessions.ConfirmFilters(session, previous);
                }
            }

            throw;
        }
        catch (Exception exception)
        {
            if (!committed
                && previous is not null
                && sessions.IsCurrent(session))
            {
                ApplyToSdk(session.Player.Filters, previous);

                if (!dispatched)
                {
                    sessions.ConfirmFilters(session, previous);
                }
            }

            logger.LogWarning(exception,
                "Filter operation failed. Guild={GuildId} Operation={Operation} Dispatched={Dispatched}",
                guildId, operation, dispatched);
            return Reject(guildId, dispatched
                ? LocalizationKeys.FiltersCommandOutcomeUnknown
                : LocalizationKeys.FiltersCommandOperationFailed);
        }
        finally
        {
            if (reservation is not null)
            {
                sessions.ReleaseControl(reservation);
            }
        }
    }

    /// <summary>Reconciles pending updates from an authoritative guild player read and synchronizes the SDK cache.</summary>
    private async Task<PlaybackFilterState> ReadRemoteAsync(
        PlaybackSession session,
        ControlReservation reservation,
        CancellationToken cancellationToken)
    {
        var player = await session.Player.ApiClient.GetPlayerAsync(
            session.Player.SessionId, session.GuildId, cancellationToken);
        sessions.ValidateControl(session, reservation);

        if (player is null || player.GuildId != session.GuildId)
        {
            throw new InvalidOperationException("The remote filter snapshot belongs to another guild.");
        }

        var observed = Snapshot(player.Filters ?? new());
        ApplyToSdk(session.Player.Filters, observed);
        sessions.ConfirmFilters(session, observed);
        return observed;
    }

    /// <summary>Refreshes persisted source position after a timescale change without touching a replacement occurrence.</summary>
    private async Task<bool> RefreshPositionAsync(
        PlaybackSession session,
        ControlReservation reservation,
        long? queueItemId,
        CancellationToken cancellationToken)
    {
        if (!queueItemId.HasValue)
        {
            return true;
        }

        try
        {
            var remote = await session.Player.ApiClient.GetPlayerAsync(
                session.Player.SessionId, session.GuildId, cancellationToken);
            sessions.ValidateControl(session, reservation);

            if (remote is null || remote.GuildId != session.GuildId)
            {
                throw new InvalidOperationException("Could not confirm the position after applying filters.");
            }

            return await sessions.MutateAsync(session, async () =>
            {
                sessions.ValidateControl(session, reservation);

                var currentTrack = session.Player.CurrentTrack;

                if (sessions.GetReadyQueueItemId(session) != queueItemId
                    || currentTrack is null
                    || remote.CurrentTrack?.Information.Identifier != currentTrack.Identifier
                    || remote.CurrentTrack.Information.SourceName != currentTrack.SourceName)
                {
                    return true;
                }

                var position = TimeSpan.FromTicks(Math.Clamp(
                    remote.CurrentTrack.Information.Position.Ticks, 0, currentTrack.Duration.Ticks));
                await playbackStateRepository.SetPlaybackPositionAsync(
                    session.GuildId, position, remote.IsPaused,
                    MobileRealtimeEventNames.PlaybackPositionChanged, cancellationToken);
                return true;
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Filters applied but position checkpoint failed. Guild={GuildId}", session.GuildId);
            return false;
        }
    }

    /// <summary>Caches supported filter names per API client, node session, and reported server version for one minute.</summary>
    private async Task<ImmutableHashSet<string>> GetSupportedFiltersAsync(ILavalinkPlayer player, CancellationToken token)
    {
        if (_capabilities.TryGetValue(player.ApiClient, out var cached)
            && cached.SessionId == player.SessionId
            && cached.Expires > DateTimeOffset.UtcNow)
        {
            return cached.Filters;
        }

        var information = await player.ApiClient.RetrieveServerInformationAsync(token);
        var available = information.AvailableFilters.IsDefault
            ? ImmutableHashSet<string>.Empty
            : information.AvailableFilters.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
        _capabilities[player.ApiClient] = (player.SessionId, information.Version.ToString(), DateTimeOffset.UtcNow.AddMinutes(1), available);
        return available;
    }

    /// <summary>Checks all configured built-in filters against this node's advertised capabilities.</summary>
    private static bool IsSupported(PlaybackFilterState snapshot, ImmutableHashSet<string> supported)
    {
        return ConfiguredNames(snapshot.Filters).All(supported.Contains);
    }

    /// <summary>Names configured built-in filters using the Lavalink capability identifiers.</summary>
    private static IEnumerable<string> ConfiguredNames(PlayerFilterMapModel filters)
    {
        return Options(filters).Where(option => !option.Option.IsDefault)
            .Select(option => option.Name);
    }

    /// <summary>Copies all filter state, including plugin JSON values, into immutable snapshot containers.</summary>
    private static PlaybackFilterState Snapshot(PlayerFilterMapModel model)
    {
        var normalized = new PlayerFilterMapModel();

        foreach (var (_, option) in Options(model))
        {
            if (!option.IsDefault)
            {
                option.Apply(ref normalized);
            }
        }

        var additional = model.AdditionalFilters?.ToImmutableDictionary(
            item => item.Key, item => item.Value.Clone(), StringComparer.Ordinal);
        return new PlaybackFilterState(normalized with { AdditionalFilters = additional });
    }

    /// <summary>Maps protocol filter values to immutable SDK options while preserving every unrelated built-in setting.</summary>
    private static IEnumerable<(string Name, IFilterOptions Option)> Options(PlayerFilterMapModel model)
    {
        if (model.Volume is { } volume)
        {
            yield return ("volume", new VolumeFilterOptions(volume));
        }

        if (model.Equalizer is { } bands && !bands.IsDefaultOrEmpty)
        {
            var equalizer = Equalizer.CreateBuilder();

            foreach (var band in bands)
            {
                equalizer[band.Band] = band.Gain;
            }

            yield return ("equalizer", new EqualizerFilterOptions(equalizer.Build()));
        }

        if (model.Timescale is { } timescale)
        {
            yield return ("timescale", new TimescaleFilterOptions(timescale.Speed, timescale.Pitch, timescale.Rate));
        }

        if (model.Karaoke is { } karaoke)
        {
            yield return ("karaoke", new KaraokeFilterOptions(karaoke.Level, karaoke.MonoLevel, karaoke.FilterBand, karaoke.FilterWidth));
        }

        if (model.Rotation is { } rotation)
        {
            yield return ("rotation", new RotationFilterOptions(rotation.Frequency));
        }

        if (model.ChannelMix is { } mix)
        {
            yield return ("channelMix", new ChannelMixFilterOptions(mix.LeftToLeft, mix.LeftToRight, mix.RightToLeft, mix.RightToRight));
        }

        if (model.LowPass is { } lowPass)
        {
            yield return ("lowPass", new LowPassFilterOptions(lowPass.SmoothingFactor));
        }

        if (model.Tremolo is { } tremolo)
        {
            yield return ("tremolo", new TremoloFilterOptions(tremolo.Frequency, tremolo.Depth));
        }

        if (model.Vibrato is { } vibrato)
        {
            yield return ("vibrato", new VibratoFilterOptions(vibrato.Frequency, vibrato.Depth));
        }

        if (model.Distortion is { } distortion)
        {
            yield return ("distortion", new DistortionFilterOptions(
                distortion.SinOffset, distortion.SinScale, distortion.CosOffset, distortion.CosScale,
                distortion.TanOffset, distortion.TanScale, distortion.Offset, distortion.Scale));
        }
    }

    /// <summary>Stages a complete snapshot in the SDK without sending a request or changing player output volume.</summary>
    private static void ApplyToSdk(IPlayerFilters filters, PlaybackFilterState snapshot)
    {
        filters.Clear();

        foreach (var (_, option) in Options(snapshot.Filters))
        {
            switch (option)
            {
                case VolumeFilterOptions volume: filters.Volume = volume; break;
                case EqualizerFilterOptions equalizer: filters.Equalizer = equalizer; break;
                case TimescaleFilterOptions timescale: filters.Timescale = timescale; break;
                case KaraokeFilterOptions karaoke: filters.Karaoke = karaoke; break;
                case RotationFilterOptions rotation: filters.Rotation = rotation; break;
                case ChannelMixFilterOptions mix: filters.ChannelMix = mix; break;
                case LowPassFilterOptions lowPass: filters.LowPass = lowPass; break;
                case TremoloFilterOptions tremolo: filters.Tremolo = tremolo; break;
                case VibratoFilterOptions vibrato: filters.Vibrato = vibrato; break;
                case DistortionFilterOptions distortion: filters.Distortion = distortion; break;
            }
        }

        if (snapshot.Filters.AdditionalFilters is { Count: > 0 } additional)
        {
            filters.SetFilter(new PluginFilterOptions(additional));
        }
    }

    /// <summary>Creates conservative named presets; preset selection replaces the whole map and never changes output volume.</summary>
    private static PlaybackFilterState? CreatePreset(string name)
    {
        PlayerFilterMapModel? filters = name switch
        {
            "flat" => new(),
            "bass-light" => new() { Equalizer = BassBands(0.06f) },
            "bass-heavy" => new() { Equalizer = BassBands(0.20f), Volume = 0.85f },
            "nightcore" => new() { Timescale = new TimescaleFilterModel(1.15f, 1.15f, 1f) },
            "slowed" => new() { Timescale = new TimescaleFilterModel(0.85f, 0.9f, 1f) },
            "karaoke" => new() { Karaoke = new KaraokeFilterModel(1f, 1f, 220f, 100f) },
            "rotation" => new() { Rotation = new RotationFilterModel(0.2f) },
            "mono" => new() { ChannelMix = new ChannelMixFilterModel(0.5f, 0.5f, 0.5f, 0.5f) },
            "soft" => new() { LowPass = new LowPassFilterModel(20f) },
            _ => null
        };
        return filters is null ? null : Snapshot(filters);
    }

    /// <summary>Builds a bounded low-frequency curve without introducing an unrelated filter.</summary>
    private static ImmutableArray<EqualizerBandModel>? BassBands(float gain)
    {
        return gain == 0 ? null :
        [new EqualizerBandModel(0, gain), new EqualizerBandModel(1, gain * 0.8f),
            new EqualizerBandModel(2, gain * 0.6f), new EqualizerBandModel(3, gain * 0.3f)];
    }

    /// <summary>Formats confirmed numeric values without exposing SDK type names or private protocol state.</summary>
    private string FormatState(ulong guildId, PlaybackFilterState snapshot, float outputVolume)
    {
        var filters = snapshot.Filters;
        var none = localizationService.Get(guildId, LocalizationKeys.PlaybackDetailsNone);
        var names = ConfiguredNames(filters).Concat(filters.AdditionalFilters?.Keys ?? [])
            .ToArray();
        var equalizer = filters.Equalizer is { } bands
            ? string.Join(", ", bands.OrderBy(band => band.Band)
                .Select(band => $"{band.Band}: {Number(band.Gain)}"))
            : none;
        var description = localizationService.Get(guildId, LocalizationKeys.FiltersCommandState,
            Number(outputVolume * 100f), Number(filters.Timescale?.Speed ?? 1f),
            Number(filters.Timescale?.Pitch ?? 1f), Number(filters.Timescale?.Rate ?? 1f),
            Number((filters.Volume ?? 1f) * 100f), equalizer, names.Length == 0 ? none : string.Join(", ", names));

        foreach (var (_, option) in Options(filters))
        {
            if (option is KaraokeFilterOptions karaoke)
            {
                description += localizationService.Get(guildId, LocalizationKeys.FiltersDetailsKaraoke,
                    Number(karaoke.Level ?? 1f), Number(karaoke.MonoLevel ?? 1f),
                    Number(karaoke.FilterBand ?? 220f), Number(karaoke.FilterWidth ?? 100f));
            }
            else if (option is RotationFilterOptions rotation)
            {
                description += localizationService.Get(guildId, LocalizationKeys.FiltersDetailsRotation, Number(rotation.Frequency ?? 0f));
            }
            else if (option is LowPassFilterOptions lowPass)
            {
                description += localizationService.Get(guildId, LocalizationKeys.FiltersDetailsLowPass, Number(lowPass.Smoothing ?? 0f));
            }
            else if (option is ChannelMixFilterOptions mix)
            {
                description += localizationService.Get(guildId, LocalizationKeys.FiltersDetailsChannelMix,
                    Number(mix.LeftToLeft ?? 1f), Number(mix.LeftToRight ?? 0f),
                    Number(mix.RightToLeft ?? 0f), Number(mix.RightToRight ?? 1f));
            }
            else if (option is TremoloFilterOptions tremolo)
            {
                description += localizationService.Get(guildId, LocalizationKeys.FiltersDetailsTremolo,
                    Number(tremolo.Frequency ?? 2f), Number(tremolo.Depth ?? 0.5f));
            }
            else if (option is VibratoFilterOptions vibrato)
            {
                description += localizationService.Get(guildId, LocalizationKeys.FiltersDetailsVibrato,
                    Number(vibrato.Frequency ?? 2f), Number(vibrato.Depth ?? 0.5f));
            }
            else if (option is DistortionFilterOptions distortion)
            {
                description += localizationService.Get(guildId, LocalizationKeys.FiltersDetailsDistortion,
                    Number(distortion.SinOffset ?? 0f), Number(distortion.SinScale ?? 1f),
                    Number(distortion.CosOffset ?? 0f), Number(distortion.CosScale ?? 1f),
                    Number(distortion.TanOffset ?? 0f), Number(distortion.TanScale ?? 1f),
                    Number(distortion.Offset ?? 0f), Number(distortion.Scale ?? 1f));
            }
        }

        return description;
    }

    /// <summary>Formats protocol numbers with invariant decimal separators.</summary>
    private static string Number(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    /// <summary>Returns a localized validation or uncertainty result through the existing playback contract.</summary>
    private PlaybackControlResult Reject(ulong guildId, string key)
    {
        return PlaybackControlResult.Failed(localizationService.Get(guildId, key), key);
    }

    /// <summary>Validates one bounded advanced effect operation without mutating SDK or persisted state.</summary>
    internal static bool TryCreateAdvancedMutation(
        IReadOnlyList<string> arguments,
        out Func<PlaybackFilterState, PlaybackFilterState>? mutation)
    {
        mutation = null;

        if (arguments.Count == 0)
        {
            return false;
        }

        var effect = arguments[0].ToLowerInvariant() switch
        {
            "equalizer" => "eq",
            "channel-mix" => "channelmix",
            "low-pass" => "lowpass",
            var name => name
        };

        if (effect is not ("eq" or "rate" or "karaoke" or "rotation" or "channelmix"
            or "lowpass" or "tremolo" or "vibrato" or "distortion"))
        {
            return false;
        }

        if (arguments.Count == 1)
        {
            return true;
        }

        if (arguments.Count == 2
            && arguments[1].ToLowerInvariant() is "off" or "false")
        {
            mutation = current => Snapshot(effect switch
            {
                "eq" => current.Filters with { Equalizer = null },
                "rate" => current.Filters with
                {
                    Timescale = (current.Filters.Timescale ?? new TimescaleFilterModel(1f, 1f, 1f)) with { Rate = 1f }
                },
                "karaoke" => current.Filters with { Karaoke = null },
                "rotation" => current.Filters with { Rotation = null },
                "channelmix" => current.Filters with { ChannelMix = null },
                "lowpass" => current.Filters with { LowPass = null },
                "tremolo" => current.Filters with { Tremolo = null },
                "vibrato" => current.Filters with { Vibrato = null },
                _ => current.Filters with { Distortion = null }
            });
            return true;
        }

        if (effect == "karaoke"
            && arguments.Count == 2
            && arguments[1].ToLowerInvariant() is "on" or "true")
        {
            mutation = current => Snapshot(current.Filters with { Karaoke = new KaraokeFilterModel(1f, 1f, 220f, 100f) });
            return true;
        }

        var parsedValues = new double[arguments.Count - 1];

        for (var index = 0; index < parsedValues.Length; index++)
        {
            if (!double.TryParse(arguments[index + 1], NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out var value)
                || !double.IsFinite(value)
                || value < -20000
                || value > 20000)
            {
                return false;
            }

            parsedValues[index] = value;
        }

        var valid = effect switch
        {
            "eq" => parsedValues.Length == 2 && InRange(parsedValues[0], 0, 14)
                && parsedValues[0] == Math.Truncate(parsedValues[0]) && InRange(parsedValues[1], -0.25, 0.25),
            "rate" => parsedValues.Length == 1 && InRange(parsedValues[0], 0.5, 2),
            "karaoke" => parsedValues.Length == 4 && InRange(parsedValues[0], 0, 1) && InRange(parsedValues[1], 0, 1)
                && InRange(parsedValues[2], 20, 20000) && InRange(parsedValues[3], 1, 20000),
            "rotation" => parsedValues.Length == 1 && InRange(parsedValues[0], -1, 1),
            "channelmix" => parsedValues.Length == 4 && parsedValues.All(value => InRange(value, 0, 1)),
            "lowpass" => parsedValues.Length == 1 && InRange(parsedValues[0], 1.01, 100),
            "tremolo" => parsedValues.Length == 2 && InRange(parsedValues[0], 0.1, 20) && InRange(parsedValues[1], 0.01, 1),
            "vibrato" => parsedValues.Length == 2 && InRange(parsedValues[0], 0.1, 14) && InRange(parsedValues[1], 0.01, 1),
            "distortion" => parsedValues.Length == 8
                && InRange(parsedValues[0], -1, 1) && InRange(parsedValues[2], -1, 1)
                && InRange(parsedValues[4], -1, 1) && InRange(parsedValues[6], -1, 1)
                && InRange(parsedValues[1], 0, 2) && InRange(parsedValues[3], 0, 2)
                && InRange(parsedValues[5], 0, 2) && InRange(parsedValues[7], 0, 2),
            _ => false
        };

        if (!valid)
        {
            return false;
        }

        var values = parsedValues.Select(value => (float)value)
            .ToArray();

        mutation = current => Snapshot(effect switch
        {
            "eq" => current.Filters with
            {
                Equalizer = (current.Filters.Equalizer ?? []).Where(band => band.Band != (int)values[0])
                    .Append(new EqualizerBandModel((int)values[0], values[1]))
                    .OrderBy(band => band.Band)
                    .ToImmutableArray()
            },
            "rate" => current.Filters with
            {
                Timescale = (current.Filters.Timescale ?? new TimescaleFilterModel(1f, 1f, 1f)) with { Rate = values[0] }
            },
            "karaoke" => current.Filters with { Karaoke = new KaraokeFilterModel(values[0], values[1], values[2], values[3]) },
            "rotation" => current.Filters with { Rotation = new RotationFilterModel(values[0]) },
            "channelmix" => current.Filters with { ChannelMix = new ChannelMixFilterModel(values[0], values[1], values[2], values[3]) },
            "lowpass" => current.Filters with { LowPass = new LowPassFilterModel(values[0]) },
            "tremolo" => current.Filters with { Tremolo = new TremoloFilterModel(values[0], values[1]) },
            "vibrato" => current.Filters with { Vibrato = new VibratoFilterModel(values[0], values[1]) },
            _ => current.Filters with
            {
                Distortion = new DistortionFilterModel(values[0], values[1], values[2], values[3],
                    values[4], values[5], values[6], values[7])
            }
        });
        return true;
    }

    /// <summary>Checks an inclusive application range after finite invariant parsing.</summary>
    private static bool InRange(double value, double minimum, double maximum) => value >= minimum && value <= maximum;

    /// <summary>Lists, saves, applies or deletes a guild-local custom preset with explicit management authorization.</summary>
    private async Task<PlaybackControlResult> CustomAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string[] arguments,
        CancellationToken token)
    {
        var guildId = message.Channel.Guild.Id;
        var operation = arguments.Length == 0 ? "list" : arguments[0].ToLowerInvariant();

        if (operation == "list")
        {
            var page = 1;

            if (arguments.Length > 2
                || (arguments.Length == 2
                    && (!int.TryParse(arguments[1], NumberStyles.None, CultureInfo.InvariantCulture, out page)
                        || page is < 1 or > 1000000)))
            {
                return Reject(guildId, LocalizationKeys.FilterPresetInvalid);
            }

            var request = new PaginationRequest(async (number, cancellation) =>
            {
                var total = await presetRepository.CountAsync(guildId, cancellation);
                var pages = Math.Max(1, (int)Math.Ceiling(total / 10d));
                var selected = Math.Clamp(number, 1, pages);
                var presets = await presetRepository.ListAsync(guildId, selected, cancellation);
                return new PaginationPage(localizationService.Get(guildId, LocalizationKeys.PaginationPresetsTitle),
                    string.Join("\n", presets.Select(preset => preset.Name)), selected, pages, total);
            }, page);
            var initial = await request.LoadPageAsync(page, token);
            return PlaybackControlResult.Succeeded(localizationService.Get(guildId, LocalizationKeys.FilterPresetList,
                initial.Number, initial.Description)) with
            {
                Pagination = request with { InitialContent = initial }
            };
        }

        if (arguments.Length is < 2 or > 3
            || !TryNormalizePresetName(arguments[1], out var name)
            || operation is not ("save" or "apply" or "delete")
            || (arguments.Length == 3
                && (operation != "save"
                    || !arguments[2].Equals("overwrite", StringComparison.OrdinalIgnoreCase))))
        {
            return Reject(guildId, LocalizationKeys.FilterPresetInvalid);
        }

        if (operation == "apply")
        {
            var preset = await presetRepository.GetAsync(guildId, name, token);

            if (preset is null)
            {
                return Reject(guildId, LocalizationKeys.FilterPresetMissing);
            }

            if (!TryReadCustomPreset(preset.SchemaVersion, preset.DefinitionJson, out var snapshot))
            {
                return Reject(guildId, LocalizationKeys.FilterPresetUnsupported);
            }

            return await ExecuteAsync(message, member, "custom-preset", _ => snapshot!, name, token);
        }

        var administrator = await message.Channel.Guild.GetMemberAsync(message.Author.Id)
            .WaitAsync(token);
        var permissions = administrator.ToDiscordMember().Permissions;

        if (!CanManagePreset(permissions))
        {
            return Reject(guildId, LocalizationKeys.FilterPresetForbidden);
        }

        if (operation == "delete")
        {
            return await presetRepository.DeleteAsync(guildId, name, token)
                ? PlaybackControlResult.Succeeded(localizationService.Get(guildId, LocalizationKeys.FilterPresetDeleted, name))
                : Reject(guildId, LocalizationKeys.FilterPresetMissing);
        }

        var session = sessions.CaptureCurrent(guildId);

        if (session is null
            || administrator.VoiceState?.Channel?.Id != session.Player.VoiceChannelId)
        {
            return Reject(guildId, LocalizationKeys.PlaybackWrongVoiceChannel);
        }

        ControlReservation? reservation = null;
        try
        {
            reservation = sessions.ReserveControl(session, "Filters.SavePreset");
            var connection = await validationService.ValidateConnectionAsync(session.Player, token);
            sessions.ValidateControl(session, reservation);

            if (!connection.IsValid)
            {
                return Reject(guildId, ValidationErrorKeys.BotIsNotConnectedError);
            }

            var snapshot = await ReadRemoteAsync(session, reservation, token);

            if (!IsValidCustomState(snapshot.Filters))
            {
                return Reject(guildId, LocalizationKeys.FilterPresetUnsupported);
            }

            var definition = JsonSerializer.Serialize(snapshot.Filters);
            var saved = await sessions.MutateAsync(session, () => presetRepository.SaveAsync(
                guildId, name, 1, definition, message.Author.Id, arguments.Length == 3, token));
            return saved
                ? PlaybackControlResult.Succeeded(localizationService.Get(guildId, LocalizationKeys.FilterPresetSaved, name))
                : Reject(guildId, LocalizationKeys.FilterPresetExists);
        }
        catch (PlaybackSessionExpiredException)
        {
            return Reject(guildId, ValidationErrorKeys.PlaybackSuperseded);
        }
        catch (PlaybackOperationBusyException)
        {
            return Reject(guildId, ValidationErrorKeys.PlaybackBusy);
        }
        finally
        {
            if (reservation is not null)
            {
                sessions.ReleaseControl(reservation);
            }
        }
    }

    /// <summary>Normalizes bounded, guild-local names without accepting separators or arbitrary JSON arguments.</summary>
    internal static bool TryNormalizePresetName(string value, out string name)
    {
        name = value.Normalize(NormalizationForm.FormKC)
            .Trim()
            .ToLowerInvariant();
        return name.Length is >= 1 and <= 32
            && name.All(character => char.IsLetterOrDigit(character) || character is '-' or '_');
    }

    /// <summary>Rejects incompatible, malformed, plugin-specific or out-of-range definitions before any player mutation.</summary>
    internal static bool TryReadCustomPreset(int version, string definition, out PlaybackFilterState? snapshot)
    {
        snapshot = null;

        if (version != 1
            || definition.Length is 0 or > 32768)
        {
            return false;
        }

        try
        {
            var filters = JsonSerializer.Deserialize<PlayerFilterMapModel>(definition);

            if (filters is null || !IsValidCustomState(filters))
            {
                return false;
            }

            snapshot = Snapshot(filters);
            return true;
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>Validates version-one built-in effect definitions using the same ranges as the command surface.</summary>
    private static bool IsValidCustomState(PlayerFilterMapModel filters)
    {
        if (filters.AdditionalFilters is { Count: > 0 }
            || (filters.Volume is { } volume
                && (!float.IsFinite(volume)
                    || !InRange(volume, 0, 1))))
        {
            return false;
        }

        if (filters.Equalizer is { } bands)
        {
            if (bands.IsDefault
                || bands.Length > 15
                || bands.Select(band => band.Band)
                    .Distinct()
                    .Count() != bands.Length
                || bands.Any(band => band.Band is < 0 or > 14
                    || !float.IsFinite(band.Gain)
                    || !InRange(band.Gain, -0.25, 0.25)))
            {
                return false;
            }
        }

        var checks = new List<string[]>();

        if (filters.Timescale is { } timescale)
        {
            if (!float.IsFinite(timescale.Speed ?? 1f)
                || !InRange(timescale.Speed ?? 1f, 0.5, 2)
                || !float.IsFinite(timescale.Pitch ?? 1f)
                || !InRange(timescale.Pitch ?? 1f, 0.5, 2))
            {
                return false;
            }

            checks.Add(["rate", PresetNumber(timescale.Rate ?? 1f)]);
        }

        if (filters.Karaoke is { } karaoke)
        {
            checks.Add(["karaoke", PresetNumber(karaoke.Level ?? 1f), PresetNumber(karaoke.MonoLevel ?? 1f), PresetNumber(karaoke.FilterBand ?? 220f), PresetNumber(karaoke.FilterWidth ?? 100f)]);
        }

        if (filters.Rotation is { } rotation)
        {
            checks.Add(["rotation", PresetNumber(rotation.Frequency ?? 0f)]);
        }

        if (filters.ChannelMix is { } mix)
        {
            checks.Add(["channelmix", PresetNumber(mix.LeftToLeft ?? 1f), PresetNumber(mix.LeftToRight ?? 0f), PresetNumber(mix.RightToLeft ?? 0f), PresetNumber(mix.RightToRight ?? 1f)]);
        }

        if (filters.LowPass is { } lowPass)
        {
            checks.Add(["lowpass", PresetNumber(lowPass.SmoothingFactor ?? 0f)]);
        }

        if (filters.Tremolo is { } tremolo)
        {
            checks.Add(["tremolo", PresetNumber(tremolo.Frequency ?? 2f), PresetNumber(tremolo.Depth ?? 0.5f)]);
        }

        if (filters.Vibrato is { } vibrato)
        {
            checks.Add(["vibrato", PresetNumber(vibrato.Frequency ?? 2f), PresetNumber(vibrato.Depth ?? 0.5f)]);
        }

        if (filters.Distortion is { } distortion)
        {
            checks.Add(["distortion", PresetNumber(distortion.SinOffset ?? 0f), PresetNumber(distortion.SinScale ?? 1f),
                PresetNumber(distortion.CosOffset ?? 0f), PresetNumber(distortion.CosScale ?? 1f), PresetNumber(distortion.TanOffset ?? 0f),
                PresetNumber(distortion.TanScale ?? 1f), PresetNumber(distortion.Offset ?? 0f), PresetNumber(distortion.Scale ?? 1f)]);
        }

        return checks.All(check => TryCreateAdvancedMutation(check, out _));
    }

    /// <summary>Checks guild management rather than channel-level permissions or voice membership alone.</summary>
    internal static bool CanManagePreset(DiscordPermissions permissions) =>
        permissions.HasPermission(DiscordPermission.ManageGuild)
        || permissions.HasPermission(DiscordPermission.Administrator);

    /// <summary>Preserves numeric precision while validating a stored definition.</summary>
    private static string PresetNumber(float value) => value.ToString("R", CultureInfo.InvariantCulture);
}
