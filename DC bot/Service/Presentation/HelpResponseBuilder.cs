using System.Globalization;
using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Helper;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Interface.Service.Presentation.Models;
using DSharpPlus.Entities;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Presentation;

/// <summary>Owns help metadata, typed view navigation and native V2 presentation without sending Discord messages.</summary>
/// <param name="commands">Supplies authoritative registered command names and description keys.</param>
/// <param name="localization">Resolves help content in the requesting guild language.</param>
/// <param name="settings">Supplies the configured text-command prefix.</param>
public sealed class HelpResponseBuilder(
    ICommandRegistry commands,
    ILocalizationService localization,
    IOptions<BotSettings> settings) : IHelpResponseBuilder
{
    private const int CommandsPerPage = 7;
    private const int MaxBodyLength = 3500;
    private static readonly DiscordColor OverviewColor = new(0x5865f2);
    private static readonly DiscordColor DetailsColor = new(0xae76ff);
    private static readonly IReadOnlyDictionary<HelpCategory, (string NameKey, string DescriptionKey)> Categories =
        new Dictionary<HelpCategory, (string, string)>
        {
            [HelpCategory.Playback] = (LocalizationKeys.HelpPlaybackName, LocalizationKeys.HelpPlaybackDescription),
            [HelpCategory.Queue] = (LocalizationKeys.HelpQueueName, LocalizationKeys.HelpQueueDescription),
            [HelpCategory.Playlists] = (LocalizationKeys.HelpPlaylistsName, LocalizationKeys.HelpPlaylistsDescription),
            [HelpCategory.Audio] = (LocalizationKeys.HelpAudioName, LocalizationKeys.HelpAudioDescription),
            [HelpCategory.Discovery] = (LocalizationKeys.HelpDiscoveryName, LocalizationKeys.HelpDiscoveryDescription),
            [HelpCategory.Settings] = (LocalizationKeys.HelpSettingsName, LocalizationKeys.HelpSettingsDescription),
            [HelpCategory.Utilities] = (LocalizationKeys.HelpUtilitiesName, LocalizationKeys.HelpUtilitiesDescription)
        };
    private static readonly IReadOnlyDictionary<string, (HelpCategory Category, string RequirementsKey, string ExampleKey)> Definitions =
        new Dictionary<string, (HelpCategory, string, string)>(StringComparer.OrdinalIgnoreCase)
        {
            [CommandNames.AddSongToPlaylist] = (HelpCategory.Playlists, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExamplePlaylistImport),
            [CommandNames.Autoplay] = (HelpCategory.Discovery, LocalizationKeys.HelpRequiresAutoplay, LocalizationKeys.HelpExampleStatus),
            [CommandNames.BassBoost] = (HelpCategory.Audio, LocalizationKeys.HelpRequiresAudio, LocalizationKeys.HelpExampleBass),
            [CommandNames.Clear] = (HelpCategory.Queue, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleDefault),
            [CommandNames.CreatePlaylist] = (HelpCategory.Playlists, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExamplePlaylistNameCommand),
            [CommandNames.DeletePlaylist] = (HelpCategory.Playlists, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExamplePlaylistNameCommand),
            [CommandNames.Favorite] = (HelpCategory.Discovery, LocalizationKeys.HelpRequiresFavorite, LocalizationKeys.HelpExampleList),
            [CommandNames.Filters] = (HelpCategory.Audio, LocalizationKeys.HelpRequiresFilters, LocalizationKeys.HelpExampleShow),
            [CommandNames.Forward] = (HelpCategory.Playback, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleSeconds),
            [CommandNames.Help] = (HelpCategory.Utilities, LocalizationKeys.HelpRequiresServer, LocalizationKeys.HelpExampleHelp),
            [CommandNames.History] = (HelpCategory.Utilities, LocalizationKeys.HelpRequiresServer, LocalizationKeys.HelpExampleDefault),
            [CommandNames.ImportPlaylist] = (HelpCategory.Playlists, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExamplePlaylistImport),
            [CommandNames.Join] = (HelpCategory.Settings, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleDefault),
            [CommandNames.Karaoke] = (HelpCategory.Audio, LocalizationKeys.HelpRequiresAudio, LocalizationKeys.HelpExampleKaraoke),
            [CommandNames.Language] = (HelpCategory.Settings, LocalizationKeys.HelpRequiresServer, LocalizationKeys.HelpExampleLanguage),
            [CommandNames.Leave] = (HelpCategory.Settings, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleDefault),
            [CommandNames.ListPlaylists] = (HelpCategory.Playlists, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleDefault),
            [CommandNames.LoadPlaylist] = (HelpCategory.Playlists, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExamplePlaylistNameCommand),
            [CommandNames.Lyrics] = (HelpCategory.Discovery, LocalizationKeys.HelpRequiresServer, LocalizationKeys.HelpExampleTrackQuery),
            [CommandNames.Move] = (HelpCategory.Queue, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleQueueMove),
            [CommandNames.MusicConfig] = (HelpCategory.Settings, LocalizationKeys.HelpRequiresManageServer, LocalizationKeys.HelpExampleShow),
            [CommandNames.MusicStatus] = (HelpCategory.Utilities, LocalizationKeys.HelpRequiresServer, LocalizationKeys.HelpExampleDefault),
            [CommandNames.NowPlaying] = (HelpCategory.Playback, LocalizationKeys.HelpRequiresServer, LocalizationKeys.HelpExampleDefault),
            [CommandNames.Pause] = (HelpCategory.Playback, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleDefault),
            [CommandNames.Ping] = (HelpCategory.Utilities, LocalizationKeys.HelpRequiresServer, LocalizationKeys.HelpExampleDefault),
            [CommandNames.Pitch] = (HelpCategory.Audio, LocalizationKeys.HelpRequiresAudio, LocalizationKeys.HelpExampleMultiplier),
            [CommandNames.Play] = (HelpCategory.Playback, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleTrackQuery),
            [CommandNames.PlayNext] = (HelpCategory.Queue, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleTrackQuery),
            [CommandNames.Previous] = (HelpCategory.Playback, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleDefault),
            [CommandNames.Queue] = (HelpCategory.Queue, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleDefault),
            [CommandNames.QueueFind] = (HelpCategory.Queue, LocalizationKeys.HelpRequiresServer, LocalizationKeys.HelpExampleQueueQuery),
            [CommandNames.Recommendations] = (HelpCategory.Discovery, LocalizationKeys.HelpRequiresServer, LocalizationKeys.HelpExampleTrackQuery),
            [CommandNames.Remove] = (HelpCategory.Queue, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleQueuePosition),
            [CommandNames.RemoveSongFromPlaylist] = (HelpCategory.Playlists, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExamplePlaylistRemoveTrack),
            [CommandNames.RenamePlaylist] = (HelpCategory.Playlists, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExamplePlaylistRename),
            [CommandNames.Repeat] = (HelpCategory.Playback, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleDefault),
            [CommandNames.RepeatList] = (HelpCategory.Playback, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleDefault),
            [CommandNames.Restart] = (HelpCategory.Playback, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleDefault),
            [CommandNames.Resume] = (HelpCategory.Playback, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleDefault),
            [CommandNames.Rewind] = (HelpCategory.Playback, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleSeconds),
            [CommandNames.SearchPicker] = (HelpCategory.Discovery, LocalizationKeys.HelpRequiresServer, LocalizationKeys.HelpExampleStatus),
            [CommandNames.Seek] = (HelpCategory.Playback, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExamplePosition),
            [CommandNames.Shuffle] = (HelpCategory.Queue, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExampleDefault),
            [CommandNames.Skip] = (HelpCategory.Playback, LocalizationKeys.HelpRequiresSkip, LocalizationKeys.HelpExampleDefault),
            [CommandNames.SkipTo] = (HelpCategory.Playback, LocalizationKeys.HelpRequiresSkipto, LocalizationKeys.HelpExampleQueuePosition),
            [CommandNames.Speed] = (HelpCategory.Audio, LocalizationKeys.HelpRequiresAudio, LocalizationKeys.HelpExampleMultiplier),
            [CommandNames.Tag] = (HelpCategory.Utilities, LocalizationKeys.HelpRequiresServer, LocalizationKeys.HelpExampleMember),
            [CommandNames.TwentyFourSeven] = (HelpCategory.Settings, LocalizationKeys.HelpRequiresPersistentVoice, LocalizationKeys.HelpExampleEnable),
            [CommandNames.ViewPlaylist] = (HelpCategory.Playlists, LocalizationKeys.HelpRequiresVoice, LocalizationKeys.HelpExamplePlaylistNameCommand),
            [CommandNames.Volume] = (HelpCategory.Audio, LocalizationKeys.HelpRequiresAudio, LocalizationKeys.HelpExampleVolume),
        };

    /// <inheritdoc />
    public HelpResponse? Create(ulong guildId, string? commandName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var snapshot = commands.Commands.OrderBy(command => command.Name, StringComparer.OrdinalIgnoreCase)
            .Select(command =>
            {
                var definition = Definitions.TryGetValue(command.Name, out var metadata)
                    ? metadata
                    : (HelpCategory.Utilities, LocalizationKeys.HelpRequiresServer, LocalizationKeys.HelpExampleDefault);
                return new HelpCommandMetadata(command.Name, command.DescriptionKey,
                    definition.Item1, definition.Item2, definition.Item3);
            })
            .ToArray();
        var catalog = Array.AsReadOnly(snapshot);

        if (commandName is null)
        {
            return CreateView(guildId, catalog, new HelpNavigationState(HelpView.Home));
        }

        var selected = snapshot.FirstOrDefault(command => command.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

        if (selected is null)
        {
            return null;
        }

        var category = InCategory(catalog, selected.Category);
        var page = Array.IndexOf(category, selected) / CommandsPerPage + 1;
        return CreateView(guildId, catalog, new HelpNavigationState(HelpView.Details, selected.Category, page, selected.Name));
    }

    /// <inheritdoc />
    public HelpResponse? Navigate(
        HelpResponse response,
        HelpNavigationAction action,
        string? selection,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var state = response.Navigation;

        if (state.View == HelpView.Home)
        {
            if (action == HelpNavigationAction.Category
                && int.TryParse(selection, NumberStyles.None, CultureInfo.InvariantCulture, out var value)
                && Enum.IsDefined(typeof(HelpCategory), value)
                && InCategory(response.Catalog, (HelpCategory)value).Length > 0)
            {
                return CreateView(response.GuildId, response.Catalog,
                    new HelpNavigationState(HelpView.Category, (HelpCategory)value));
            }

            return null;
        }

        if (action == HelpNavigationAction.Home && selection is null)
        {
            return CreateView(response.GuildId, response.Catalog, new HelpNavigationState(HelpView.Home));
        }

        if (state.View == HelpView.Details)
        {
            return action == HelpNavigationAction.Back && selection is null
                ? CreateView(response.GuildId, response.Catalog, state with { View = HelpView.Category, CommandName = null })
                : null;
        }

        if (action == HelpNavigationAction.Command && state.Category is { } category)
        {
            var selected = InCategory(response.Catalog, category).Skip((state.CategoryPage - 1) * CommandsPerPage)
                .Take(CommandsPerPage)
                .FirstOrDefault(command => command.Name == selection);
            return selected is null ? null : CreateView(response.GuildId, response.Catalog,
                state with { View = HelpView.Details, CommandName = selected.Name });
        }

        if (selection is null
            && ((action == HelpNavigationAction.Previous && state.CategoryPage > 1)
                || (action == HelpNavigationAction.Next && state.CategoryPage < response.Page.PageCount)))
        {
            var number = state.CategoryPage + (action == HelpNavigationAction.Next ? 1 : -1);
            return CreateView(response.GuildId, response.Catalog, state with { CategoryPage = number });
        }

        return null;
    }

    /// <summary>Builds localized page text and footer from typed navigation and the captured registry snapshot.</summary>
    private HelpResponse CreateView(ulong guildId, IReadOnlyList<HelpCommandMetadata> catalog, HelpNavigationState state)
    {
        string Text(string key, params object[] values) => localization.Get(guildId, key, values);
        var prefix = settings.Value.Prefix;

        if (state.View == HelpView.Home)
        {
            var categories = Enum.GetValues<HelpCategory>().Where(category => InCategory(catalog, category).Length > 0)
                .ToArray();
            var rows = categories.Select(category =>
                $"**{Text(Categories[category].NameKey)} ({InCategory(catalog, category).Length})**\n"
                + Text(Categories[category].DescriptionKey));
            var description = Text(LocalizationKeys.HelpHomeDescription, categories.Length) + "\n" + Text(LocalizationKeys.HelpDirectHint, prefix)
                + "\n\n" + string.Join("\n\n", rows);
            var page = new PaginationPage(Text(LocalizationKeys.HelpHomeTitle), description, 1, 1, catalog.Count);
            return new HelpResponse(guildId, catalog, state, page,
                Text(LocalizationKeys.HelpHomeFooter, catalog.Count, categories.Length));
        }

        var categoryItems = InCategory(catalog, state.Category!.Value);
        var pageCount = Math.Max(1, (categoryItems.Length + CommandsPerPage - 1) / CommandsPerPage);
        state = state with { CategoryPage = Math.Clamp(state.CategoryPage, 1, pageCount) };
        var categoryName = Text(Categories[state.Category.Value].NameKey);

        if (state.View == HelpView.Category)
        {
            var rows = categoryItems.Skip((state.CategoryPage - 1) * CommandsPerPage)
                .Take(CommandsPerPage)
                .Select(command => $"**`{command.Name}`** — {Text(command.DescriptionKey)}");
            var description = Text(Categories[state.Category.Value].DescriptionKey) + "\n\n" + string.Join("\n\n", rows);
            var page = new PaginationPage(Text(LocalizationKeys.HelpCategoryTitle, categoryName), description, state.CategoryPage, pageCount, categoryItems.Length);
            return new HelpResponse(guildId, catalog, state, page,
                Text(LocalizationKeys.HelpCategoryFooter, state.CategoryPage, pageCount, categoryItems.Length, categoryName));
        }

        var selected = categoryItems.Single(command => command.Name == state.CommandName);
        var example = Text(selected.ExampleKey, prefix, selected.Name,
            Text(LocalizationKeys.HelpExamplePlaylistName), CommandNames.Queue);
        var usage = Text(selected.Name + "_command_usage", prefix);
        var usageParts = usage.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        var slashIndex = Array.FindIndex(usageParts, part => part.TrimStart().StartsWith("Slash:", StringComparison.Ordinal));
        var textUsage = string.Join("\n\n", slashIndex < 0 ? usageParts : usageParts.Take(slashIndex));
        var slashUsage = slashIndex < 0 ? string.Empty : string.Join("\n\n", usageParts.Skip(slashIndex));
        var details = Text(selected.DescriptionKey)
            + $"\n\n**{Text(LocalizationKeys.HelpPrefixUsageLabel)}**\n```\n{textUsage}\n```"
            + (slashUsage.Length == 0 ? string.Empty
                : $"\n\n**{Text(LocalizationKeys.HelpSlashUsageLabel)}**\n```\n{slashUsage}\n```")
            + $"\n\n**{Text(LocalizationKeys.HelpCategoryLabel)}**\n{categoryName}"
            + $"\n\n**{Text(LocalizationKeys.HelpRequirementsLabel)}**\n{Text(selected.RequirementsKey)}"
            + $"\n\n**{Text(LocalizationKeys.HelpExampleLabel)}**\n```\n{DiscordTextSanitizer.EscapeMentions(example)}\n```";
        var detailsPage = new PaginationPage(Text(LocalizationKeys.HelpDetailsTitle, selected.Name),
            DiscordTextSanitizer.Truncate(details, MaxBodyLength), 1, 1);
        return new HelpResponse(guildId, catalog, state, detailsPage, Text(LocalizationKeys.HelpDetailsFooter));
    }

    /// <summary>Filters the immutable metadata snapshot while retaining its sorted command order.</summary>
    private static HelpCommandMetadata[] InCategory(IReadOnlyList<HelpCommandMetadata> catalog, HelpCategory category) =>
        catalog.Where(command => command.Category == category)
            .ToArray();

    /// <inheritdoc />
    public DiscordMessageBuilder BuildMessage(HelpResponse response, string componentPrefix, bool expired)
    {
        string Text(string key, params object[] values) => localization.Get(response.GuildId, key, values);
        string Id(HelpNavigationAction action) => componentPrefix + ((int)action).ToString(CultureInfo.InvariantCulture);
        var state = response.Navigation;
        var builder = new DiscordMessageBuilder().EnableV2Components()
            .AddContainerComponent(new DiscordContainerComponent([
                new DiscordTextDisplayComponent("## " + DiscordTextSanitizer.Truncate(response.Page.Title, 256)),
                new DiscordSeparatorComponent(),
                new DiscordTextDisplayComponent(DiscordTextSanitizer.Truncate(response.Page.Description, MaxBodyLength)),
                new DiscordSeparatorComponent(),
                new DiscordTextDisplayComponent(DiscordTextSanitizer.Truncate(response.Footer, 200))
            ], color: state.View == HelpView.Details ? DetailsColor : OverviewColor));

        if (expired)
        {
            return builder;
        }

        if (state.View == HelpView.Home)
        {
            var options = Enum.GetValues<HelpCategory>().Where(category => InCategory(response.Catalog, category).Length > 0)
                .Select(category => new DiscordSelectComponentOption(
                    DiscordTextSanitizer.Truncate(Text(LocalizationKeys.HelpCategoryOption,
                        Text(Categories[category].NameKey), InCategory(response.Catalog, category).Length), 100),
                    ((int)category).ToString(CultureInfo.InvariantCulture),
                    DiscordTextSanitizer.Truncate(Text(Categories[category].DescriptionKey), 100)))
                .ToArray();

            if (options.Length > 0)
            {
                builder.AddActionRowComponent(new DiscordSelectComponent(Id(HelpNavigationAction.Category),
                    Text(LocalizationKeys.HelpChooseCategory), options));
            }

            return builder;
        }

        var buttons = new List<DiscordButtonComponent>();

        if (state.View == HelpView.Details)
        {
            buttons.Add(new DiscordButtonComponent(DiscordButtonStyle.Secondary,
                Id(HelpNavigationAction.Back), Text(LocalizationKeys.HelpBack)));
        }
        else if (response.Page.PageCount > 1)
        {
            buttons.Add(new DiscordButtonComponent(DiscordButtonStyle.Secondary, Id(HelpNavigationAction.Previous),
                Text(LocalizationKeys.HelpPrevious), state.CategoryPage <= 1));
            buttons.Add(new DiscordButtonComponent(DiscordButtonStyle.Primary, Id(HelpNavigationAction.Next),
                Text(LocalizationKeys.HelpNext), state.CategoryPage >= response.Page.PageCount));
        }

        buttons.Add(new DiscordButtonComponent(DiscordButtonStyle.Secondary,
            Id(HelpNavigationAction.Home), Text(LocalizationKeys.HelpCategories)));
        builder.AddActionRowComponent(buttons);

        if (state.View == HelpView.Category)
        {
            var options = InCategory(response.Catalog, state.Category!.Value).Skip((state.CategoryPage - 1) * CommandsPerPage)
                .Take(CommandsPerPage)
                .Select(command => new DiscordSelectComponentOption(
                    DiscordTextSanitizer.Truncate(command.Name + " — "
                        + DiscordTextSanitizer.Truncate(Text(command.DescriptionKey), 65), 100), command.Name))
                .ToArray();
            builder.AddActionRowComponent(new DiscordSelectComponent(Id(HelpNavigationAction.Command),
                Text(LocalizationKeys.HelpChooseCommand), options));
        }

        return builder;
    }
}