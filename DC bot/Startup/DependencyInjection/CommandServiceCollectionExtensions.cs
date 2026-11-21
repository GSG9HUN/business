using DC_bot.Commands.SlashCommands.Music;
using DC_bot.Commands.SlashCommands.Playlist;
using DC_bot.Commands.SlashCommands.Queue;
using DC_bot.Commands.SlashCommands.Utility;
using DC_bot.Commands.TextCommands.Music;
using DC_bot.Commands.TextCommands.Playlist;
using DC_bot.Commands.TextCommands.Queue;
using DC_bot.Commands.TextCommands.Utility;
using DC_bot.Interface;
using DC_bot.Interface.Service.SlashCommands;
using DC_bot.Service.SlashCommands;
using DC_bot.Wrapper.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup.DependencyInjection;

/// <summary>
/// Registers the command runtime dependencies at the composition root.
/// </summary>
public static class CommandServiceCollectionExtensions
{

    /// <summary>
    /// Registers the command services dependencies with the shared service collection.
    /// </summary>
    /// <param name="services">The services supplied for this operation.</param>
    /// <returns>The service collection with these registrations, allowing further composition.</returns>
    public static IServiceCollection AddCommandServices(this IServiceCollection services)
    {
        return services.AddTextCommands()
            .AddSlashCommandServices()
            .AddSlashCommandProcessor();
    }

    /// <summary>
    /// Registers the text commands dependencies with the shared service collection.
    /// </summary>
    /// <param name="services">The services supplied for this operation.</param>
    /// <returns>The service collection with these registrations, allowing further composition.</returns>
    private static IServiceCollection AddTextCommands(this IServiceCollection services)
    {
        return services.AddSingleton<ICommand, TagCommand>()
            .AddSingleton<ICommand, JoinCommand>()
            .AddSingleton<ICommand, PingCommand>()
            .AddSingleton<ICommand, HelpCommand>()
            .AddSingleton<ICommand, PlayCommand>()
            .AddSingleton<ICommand, SkipCommand>()
            .AddSingleton<ICommand, MusicConfigCommand>()
            .AddSingleton<ICommand, MusicStatusCommand>()
            .AddSingleton<ICommand, PreviousCommand>()
            .AddSingleton<ICommand, ClearCommand>()
            .AddSingleton<ICommand, LeaveCommand>()
            .AddSingleton<ICommand, PauseCommand>()
            .AddSingleton<ICommand, ResumeCommand>()
            .AddSingleton<ICommand, NowPlayingCommand>()
            .AddSingleton<ICommand, VolumeCommand>()
            .AddSingleton<ICommand, SeekCommand>()
            .AddSingleton<ICommand, ForwardCommand>()
            .AddSingleton<ICommand, RewindCommand>()
            .AddSingleton<ICommand, RestartCommand>()
            .AddSingleton<ICommand, FiltersCommand>()
            .AddSingleton<ICommand, BassBoostCommand>()
            .AddSingleton<ICommand, SpeedCommand>()
            .AddSingleton<ICommand, RemoveCommand>()
            .AddSingleton<ICommand, MoveCommand>()
            .AddSingleton<ICommand, SkipToCommand>()
            .AddSingleton<ICommand, QueueFindCommand>()
            .AddSingleton<ICommand, PlayNextCommand>()
            .AddSingleton<ICommand, LyricsCommand>()
            .AddSingleton<ICommand, FavoriteCommand>()
            .AddSingleton<ICommand, RecommendationsCommand>()
            .AddSingleton<ICommand, AutoplayCommand>()
            .AddSingleton<ICommand, SearchPickerCommand>()
            .AddSingleton<ICommand, KaraokeCommand>()
            .AddSingleton<ICommand, PitchCommand>()
            .AddSingleton<ICommand, RepeatCommand>()
            .AddSingleton<ICommand, ShuffleCommand>()
            .AddSingleton<ICommand, LanguageCommand>()
            .AddSingleton<ICommand, QueueCommand>()
            .AddSingleton<ICommand, HistoryCommand>()
            .AddSingleton<ICommand, TwentyFourSevenCommand>()
            .AddSingleton<ICommand, CreatePlaylistCommand>()
            .AddSingleton<ICommand, ImportPlaylistCommand>()
            .AddSingleton<ICommand, DeletePlaylistCommand>()
            .AddSingleton<ICommand, AddSongToPlaylistCommand>()
            .AddSingleton<ICommand, RemoveSongFromPlaylistCommand>()
            .AddSingleton<ICommand, ListPlaylistsCommand>()
            .AddSingleton<ICommand, ViewPlaylistCommand>()
            .AddSingleton<ICommand, RenamePlaylistCommand>()
            .AddSingleton<ICommand, LoadPlaylistCommand>()
            .AddSingleton<ICommand, RepeatListCommand>();
    }

    /// <summary>
    /// Registers the slash command services dependencies with the shared service collection.
    /// </summary>
    /// <param name="services">The services supplied for this operation.</param>
    /// <returns>The service collection with these registrations, allowing further composition.</returns>
    private static IServiceCollection AddSlashCommandServices(this IServiceCollection services)
    {
        return services.AddSingleton<ISlashCommandExecutor, SlashCommandExecutor>()
            .AddSingleton<ISlashInteractionContextFactory, SlashInteractionContextFactory>()
            .AddTransient<PingSlashCommand>()
            .AddTransient<HelpSlashCommand>()
            .AddTransient<TagSlashCommand>()
            .AddTransient<JoinSlashCommand>()
            .AddTransient<PlaySlashCommand>()
            .AddTransient<SkipSlashCommand>()
            .AddTransient<MusicConfigSlashCommand>()
            .AddTransient<MusicStatusSlashCommand>()
            .AddTransient<PreviousSlashCommand>()
            .AddTransient<PauseSlashCommand>()
            .AddTransient<ResumeSlashCommand>()
            .AddTransient<NowPlayingSlashCommand>()
            .AddTransient<VolumeSlashCommand>()
            .AddTransient<SeekSlashCommand>()
            .AddTransient<ForwardSlashCommand>()
            .AddTransient<RewindSlashCommand>()
            .AddTransient<RestartSlashCommand>()
            .AddTransient<FiltersSlashCommand>()
            .AddTransient<BassBoostSlashCommand>()
            .AddTransient<SpeedSlashCommand>()
            .AddTransient<RemoveSlashCommand>()
            .AddTransient<MoveSlashCommand>()
            .AddTransient<SkipToSlashCommand>()
            .AddTransient<QueueFindSlashCommand>()
            .AddTransient<PlayNextSlashCommand>()
            .AddTransient<LyricsSlashCommand>()
            .AddTransient<FavoriteSlashCommand>()
            .AddTransient<RecommendationsSlashCommand>()
            .AddTransient<AutoplaySlashCommand>()
            .AddTransient<SearchPickerSlashCommand>()
            .AddTransient<KaraokeSlashCommand>()
            .AddTransient<PitchSlashCommand>()
            .AddTransient<LeaveSlashCommand>()
            .AddTransient<QueueSlashCommand>()
            .AddTransient<HistorySlashCommand>()
            .AddTransient<ShuffleSlashCommand>()
            .AddTransient<RepeatSlashCommand>()
            .AddTransient<LanguageSlashCommand>()
            .AddTransient<ClearSlashCommand>()
            .AddTransient<PlaylistSlashCommand>()
            .AddTransient<TwentyFourSevenSlashCommand>();
    }

    /// <summary>
    /// Registers the slash command processor dependencies with the shared service collection.
    /// </summary>
    /// <param name="services">The services supplied for this operation.</param>
    /// <returns>The service collection with these registrations, allowing further composition.</returns>
    private static IServiceCollection AddSlashCommandProcessor(this IServiceCollection services)
    {
        return services.AddCommandsExtension((_, extension) =>
        {
            extension.AddCommands([
                typeof(PingSlashCommand), typeof(HelpSlashCommand), typeof(TagSlashCommand), typeof(JoinSlashCommand),
                typeof(PlaySlashCommand), typeof(SkipSlashCommand),
            typeof(MusicConfigSlashCommand),
            typeof(MusicStatusSlashCommand), typeof(PreviousSlashCommand),
                typeof(PauseSlashCommand), typeof(ResumeSlashCommand), typeof(LeaveSlashCommand),
                typeof(NowPlayingSlashCommand), typeof(VolumeSlashCommand), typeof(SeekSlashCommand),
                typeof(ForwardSlashCommand), typeof(RewindSlashCommand), typeof(RestartSlashCommand),
                typeof(FiltersSlashCommand), typeof(BassBoostSlashCommand), typeof(SpeedSlashCommand), typeof(RemoveSlashCommand),
            typeof(MoveSlashCommand),
            typeof(SkipToSlashCommand),
            typeof(QueueFindSlashCommand),
            typeof(PlayNextSlashCommand),
            typeof(LyricsSlashCommand),
            typeof(FavoriteSlashCommand),
            typeof(RecommendationsSlashCommand),
            typeof(AutoplaySlashCommand),
            typeof(SearchPickerSlashCommand),
            typeof(KaraokeSlashCommand),
            typeof(PitchSlashCommand),
                typeof(QueueSlashCommand), typeof(HistorySlashCommand), typeof(ShuffleSlashCommand),
                typeof(RepeatSlashCommand), typeof(LanguageSlashCommand), typeof(ClearSlashCommand),
                typeof(PlaylistSlashCommand), typeof(TwentyFourSevenSlashCommand)
            ]);
            extension.AddProcessor(new SlashCommandProcessor());
        }, new CommandsConfiguration { RegisterDefaultCommandProcessors = false });
    }
}
