using DC_bot.Commands.TextCommands.Music;
using DC_bot.Commands.TextCommands.Playlist;
using DC_bot.Commands.TextCommands.Queue;
using DC_bot.Commands.TextCommands.Utility;
using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.SlashCommands;
using DC_bot.Service.Core;
using DC_bot.Service.Music;
using DC_bot.Service.Presentation;
using DC_bot.Service.SlashCommands;
using Lavalink4NET.Players;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DC_bot_tests.TestHelperFiles;

internal sealed class SlashCommandTestGraph
{
    public SlashCommandTestGraph(bool useSavedGuildLanguage = false)
    {
        LocalizationServiceMock = CreateLocalizationService(useSavedGuildLanguage);
        PlayerConnectionServiceMock
            .Setup(service => service.TryJoinAndValidateAsync(
                It.IsAny<IDiscordMessage>(),
                It.IsAny<IDiscordChannel?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerMock.Object, Mock.Of<IDiscordChannel>(), 123UL, true));
        Executor = CreateExecutor(CreateCommands());
    }

    public Mock<ILavaLinkService> LavaLinkServiceMock { get; } = new();
    public Mock<ICurrentTrackService> CurrentTrackServiceMock { get; } = new();
    public Mock<IMusicQueueService> MusicQueueServiceMock { get; } = new();
    public Mock<IRepeatService> RepeatServiceMock { get; } = new();
    public Mock<ITrackFormatterService> TrackFormatterServiceMock { get; } = new();
    public Mock<ITrackSerializer> TrackSerializerMock { get; } = new();
    public Mock<IPlayerConnectionService> PlayerConnectionServiceMock { get; } = new();
    public Mock<IPlaybackEventHandlerService> PlaybackEventHandlerServiceMock { get; } = new();
    public Mock<ITrackPlaybackService> TrackPlaybackServiceMock { get; } = new();
    public Mock<ILavalinkPlayer> PlayerMock { get; } = new();
    public Mock<IPlaylistService> PlaylistServiceMock { get; } = new();
    public Mock<ILocalizationService> LocalizationServiceMock { get; }
    public ILocalizationService LocalizationService => LocalizationServiceMock.Object;
    public ISlashCommandExecutor Executor { get; }

    public ISlashCommandExecutor CreateExecutorWithCommands(params ICommand[] commands)
    {
        return CreateExecutor(commands);
    }

    private ISlashCommandExecutor CreateExecutor(IEnumerable<ICommand> commands)
    {
        return new SlashCommandExecutor(commands, Mock.Of<ILogger<SlashCommandExecutor>>(), LocalizationService);
    }

    private ICommand[] CreateCommands()
    {
        var responseBuilder = new ResponseBuilder(LocalizationService);
        var commandHelper = new CommandValidationService();
        var validationService = new ValidationService(Mock.Of<ILogger<ValidationService>>());
        var searchResolver = new TrackSearchResolverService(Options.Create(new SearchResolverOptions
        {
            DefaultQueryMode = "yt"
        }));

        return
        [
            new PingCommand(validationService, Mock.Of<ILogger<PingCommand>>(), responseBuilder, LocalizationService),
            new HelpCommand(validationService, Mock.Of<ILogger<HelpCommand>>(), responseBuilder, LocalizationService,
                CreateHelpCommandRegistry()),
            new TagCommand(validationService, Mock.Of<ILogger<TagCommand>>(), responseBuilder, LocalizationService, commandHelper),
            new JoinCommand(LavaLinkServiceMock.Object, validationService, Mock.Of<ILogger<JoinCommand>>(), responseBuilder, LocalizationService, commandHelper),
            new PlayCommand(
                LavaLinkServiceMock.Object,
                validationService,
                responseBuilder,
                Mock.Of<ILogger<PlayCommand>>(),
                searchResolver,
                LocalizationService,
                commandHelper),
            new SkipCommand(LavaLinkServiceMock.Object, validationService, Mock.Of<ILogger<SkipCommand>>(), responseBuilder, LocalizationService, commandHelper),
            new PauseCommand(LavaLinkServiceMock.Object, validationService, Mock.Of<ILogger<PauseCommand>>(), responseBuilder, LocalizationService, commandHelper),
            new ResumeCommand(LavaLinkServiceMock.Object, validationService, Mock.Of<ILogger<ResumeCommand>>(), responseBuilder, LocalizationService, commandHelper),
            new LeaveCommand(LavaLinkServiceMock.Object, validationService, Mock.Of<ILogger<LeaveCommand>>(), responseBuilder, LocalizationService, commandHelper),
            new ViewQueueCommand(MusicQueueServiceMock.Object, validationService, Mock.Of<ILogger<ViewQueueCommand>>(), responseBuilder, LocalizationService, commandHelper),
            new ShuffleCommand(validationService, MusicQueueServiceMock.Object, Mock.Of<ILogger<ShuffleCommand>>(), responseBuilder, LocalizationService, commandHelper),
            new RepeatCommand(RepeatServiceMock.Object, CurrentTrackServiceMock.Object, validationService, Mock.Of<ILogger<RepeatCommand>>(), responseBuilder, LocalizationService, commandHelper),
            new RepeatListCommand(
                RepeatServiceMock.Object,
                CurrentTrackServiceMock.Object,
                MusicQueueServiceMock.Object,
                validationService,
                Mock.Of<ILogger<RepeatListCommand>>(),
                responseBuilder,
                TrackFormatterServiceMock.Object,
                LocalizationService,
                commandHelper),
            new LanguageCommand(Mock.Of<ILogger<LanguageCommand>>(), validationService, responseBuilder, LocalizationService, commandHelper),
            new ClearCommand(validationService, MusicQueueServiceMock.Object, Mock.Of<ILogger<ClearCommand>>(), responseBuilder, LocalizationService, commandHelper),
            new CreatePlaylistCommand(
                Mock.Of<ILogger<CreatePlaylistCommand>>(),
                validationService,
                responseBuilder,
                LocalizationService,
                PlaylistServiceMock.Object,
                commandHelper),
            new SavePlaylistCommand(
                Mock.Of<ILogger<SavePlaylistCommand>>(),
                validationService,
                responseBuilder,
                LocalizationService,
                PlaylistServiceMock.Object,
                commandHelper),
            new ListPlaylistsCommand(
                Mock.Of<ILogger<ListPlaylistsCommand>>(),
                validationService,
                responseBuilder,
                LocalizationService,
                PlaylistServiceMock.Object,
                commandHelper),
            new ViewPlaylistCommand(
                Mock.Of<ILogger<ViewPlaylistCommand>>(),
                validationService,
                responseBuilder,
                LocalizationService,
                PlaylistServiceMock.Object,
                commandHelper),
            new LoadPlaylistCommand(
                Mock.Of<ILogger<LoadPlaylistCommand>>(),
                validationService,
                responseBuilder,
                LocalizationService,
                PlaylistServiceMock.Object,
                MusicQueueServiceMock.Object,
                TrackSerializerMock.Object,
                PlayerConnectionServiceMock.Object,
                PlaybackEventHandlerServiceMock.Object,
                TrackPlaybackServiceMock.Object,
                commandHelper),
            new AddSongToPlaylistCommand(
                Mock.Of<ILogger<AddSongToPlaylistCommand>>(),
                validationService,
                responseBuilder,
                LocalizationService,
                PlaylistServiceMock.Object,
                commandHelper),
            new RemoveSongFromPlaylistCommand(
                Mock.Of<ILogger<RemoveSongFromPlaylistCommand>>(),
                validationService,
                responseBuilder,
                LocalizationService,
                PlaylistServiceMock.Object,
                commandHelper),
            new RenamePlaylistCommand(
                Mock.Of<ILogger<RenamePlaylistCommand>>(),
                validationService,
                responseBuilder,
                LocalizationService,
                PlaylistServiceMock.Object,
                commandHelper),
            new DeletePlaylistCommand(
                Mock.Of<ILogger<DeletePlaylistCommand>>(),
                validationService,
                responseBuilder,
                LocalizationService,
                PlaylistServiceMock.Object,
                commandHelper)
        ];
    }

    private static CommandRegistry CreateHelpCommandRegistry()
    {
        var pingCommand = new Mock<ICommand>();
        pingCommand.SetupGet(command => command.Name).Returns("ping");
        pingCommand.SetupGet(command => command.Description).Returns("Replies with Pong!");

        var playCommand = new Mock<ICommand>();
        playCommand.SetupGet(command => command.Name).Returns("play");
        playCommand.SetupGet(command => command.Description).Returns("Plays a song");

        return new CommandRegistry(() => [pingCommand.Object, playCommand.Object]);
    }

    private static Mock<ILocalizationService> CreateLocalizationService(bool useSavedGuildLanguage)
    {
        var localizationService = new Mock<ILocalizationService>();
        var languageByGuild = new Dictionary<ulong, string>();

        localizationService
            .Setup(service => service.SaveLanguage(It.IsAny<ulong>(), It.IsAny<string>()))
            .Callback<ulong, string>((guildId, language) => languageByGuild[guildId] = language);

        localizationService
            .Setup(service => service.Get(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<string, object[]>((key, args) => FormatLocalization(key, args, "en"));

        localizationService
            .Setup(service => service.Get(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<ulong, string, object[]>((guildId, key, args) =>
                FormatLocalization(
                    key,
                    args,
                    useSavedGuildLanguage && languageByGuild.TryGetValue(guildId, out var language)
                        ? language
                        : "en"));

        return localizationService;
    }

    private static string FormatLocalization(string key, object[] args, string language)
    {
        if (language.Equals("hu", StringComparison.OrdinalIgnoreCase))
        {
            return key switch
            {
            LocalizationKeys.LanguageCommandResponse => "A nyelv sikeresen megvaltozott.",
            LocalizationKeys.PingCommandResponse => "Pong!",
            _ => FormatLocalization(key, args, "en")
            };
        }

        return key switch
        {
            LocalizationKeys.HelpCommandResponse => args.Length > 0
                ? $"Available commands:{Environment.NewLine}{args[0]}"
                : "Available commands:",
            LocalizationKeys.PingCommandResponse => "Pong!",
            LocalizationKeys.TagCommandResponse => $"Tagged: {args[0]}",
            LocalizationKeys.TagCommandUserNotExistError => $"User {args[0]} not found.",
            LocalizationKeys.ViewListCommandEmbedTitle => "Playlist",
            LocalizationKeys.ViewListCommandError => "Queue is empty.",
            LocalizationKeys.ShuffleCommandError => "There is not enough music in queue.",
            LocalizationKeys.ShuffleCommandNotEnoughTracks => "There are not enough tracks in the queue to shuffle.",
            LocalizationKeys.ShuffleCommandResponse => "The list has been shuffled.",
            LocalizationKeys.RepeatCommandListAlreadyRepeating => "The list is already repeating.",
            LocalizationKeys.RepeatCommandRepeatingOn => args.Length > 0
                ? $"Repeat is on for : {args[0]}"
                : "Repeat is on for :",
            LocalizationKeys.RepeatCommandRepeatingOff => "Repeating is off.",
            LocalizationKeys.RepeatListCommandTrackAlreadyRepeating => "This track is already repeating.",
            LocalizationKeys.RepeatListCommandRepeatingOn => args.Length > 0
                ? $"Repeat is on for current list:{Environment.NewLine}{args[0]}"
                : "Repeat is on for current list:",
            LocalizationKeys.RepeatListCommandRepeatingOff => args.Length > 0
                ? $"Repeating is off for the list:{Environment.NewLine}{args[0]}"
                : "Repeating is off for the list:",
            LocalizationKeys.LanguageCommandResponse => "The language changed successfully.",
            LocalizationKeys.ClearCommandResponse => "Playlist cleared.",
            LocalizationKeys.ClearCommandConfirmationRequired => "Set confirm to true to clear the playlist.",
            LocalizationKeys.CreatePlaylistCommandCreated => $"Playlist '{args[0]}' created.",
            LocalizationKeys.CreatePlaylistCommandAlreadyExists => $"Playlist '{args[0]}' already exists.",
            LocalizationKeys.CreatePlaylistCommandInvalidPlaylistName => "Playlist name is invalid.",
            LocalizationKeys.CreatePlaylistCommandPlaylistLimitReached => "Playlist limit reached.",
            LocalizationKeys.CreatePlaylistCommandUnknownError => $"Could not create playlist '{args[0]}'.",
            LocalizationKeys.SavePlaylistCommandSaved => $"Playlist '{args[0]}' saved.",
            LocalizationKeys.SavePlaylistCommandAlreadyExists => $"Playlist '{args[0]}' already exists.",
            LocalizationKeys.SavePlaylistCommandNoTracksFound => $"No tracks found for playlist '{args[0]}'.",
            LocalizationKeys.SavePlaylistCommandInvalidPlaylistName => "Playlist name is invalid.",
            LocalizationKeys.SavePlaylistCommandPlaylistLimitReached => "Playlist limit reached.",
            LocalizationKeys.SavePlaylistCommandTrackLimitExceeded => "Track limit exceeded.",
            LocalizationKeys.SavePlaylistCommandUnknownError => $"Could not save playlist '{args[0]}'.",
            LocalizationKeys.ListPlaylistsCommandResponse => $"Saved playlists:{Environment.NewLine}{args[0]}",
            LocalizationKeys.ListPlaylistsCommandItem => $"{args[0]}. {args[1]} - {args[2]} tracks",
            LocalizationKeys.ListPlaylistsCommandNoPlaylists => "No saved playlists.",
            LocalizationKeys.ListPlaylistsCommandUnknownError => "Could not list playlists.",
            LocalizationKeys.ViewPlaylistCommandResponse => $"Playlist '{args[0]}' ({args[1]} tracks):{Environment.NewLine}{args[2]}",
            LocalizationKeys.ViewPlaylistCommandTrack => $"{args[0]}. {args[1]} - {args[2]} ({args[3]})",
            LocalizationKeys.ViewPlaylistCommandMoreTracks => $"... and {args[0]} more tracks",
            LocalizationKeys.ViewPlaylistCommandPlaylistDoesNotExist => $"Playlist '{args[0]}' does not exist.",
            LocalizationKeys.ViewPlaylistCommandEmptyPlaylist => $"Playlist '{args[0]}' is empty.",
            LocalizationKeys.ViewPlaylistCommandInvalidPlaylistName => "Playlist name is invalid.",
            LocalizationKeys.ViewPlaylistCommandUnknownError => $"Could not view playlist '{args[0]}'.",
            LocalizationKeys.LoadPlaylistCommandLoaded => $"Playlist '{args[0]}' loaded into the queue with {args[1]} tracks.",
            LocalizationKeys.LoadPlaylistCommandNotFound => $"Playlist '{args[0]}' does not exist.",
            LocalizationKeys.LoadPlaylistCommandEmptyPlaylist => $"Playlist '{args[0]}' is empty.",
            LocalizationKeys.LoadPlaylistCommandInvalidPlaylistName => "Playlist name is invalid.",
            LocalizationKeys.LoadPlaylistCommandUnknownError => $"Could not load playlist '{args[0]}'.",
            LocalizationKeys.AddSongToPlaylistCommandAdded => $"Song added to playlist '{args[0]}'.",
            LocalizationKeys.AddSongToPlaylistCommandInvalidSongUrl => $"Song URL is invalid for playlist '{args[0]}'.",
            LocalizationKeys.AddSongToPlaylistCommandPlaylistDoesNotExist => $"Playlist '{args[0]}' does not exist.",
            LocalizationKeys.AddSongToPlaylistCommandNoTracksFound => $"No tracks found for playlist '{args[0]}'.",
            LocalizationKeys.AddSongToPlaylistCommandInvalidPlaylistName => "Playlist name is invalid.",
            LocalizationKeys.AddSongToPlaylistCommandTrackLimitReached => "Playlist track limit reached.",
            LocalizationKeys.AddSongToPlaylistCommandUnknownError => $"Could not add song to playlist '{args[0]}'.",
            LocalizationKeys.RemoveSongFromPlaylistCommandRemoved => $"Track {args[1]} removed from playlist '{args[0]}'.",
            LocalizationKeys.RemoveSongFromPlaylistCommandPlaylistDoesNotExist => $"Playlist '{args[0]}' does not exist.",
            LocalizationKeys.RemoveSongFromPlaylistCommandSongNotFound => $"Track {args[1]} was not found in playlist '{args[0]}'.",
            LocalizationKeys.RemoveSongFromPlaylistCommandInvalidPlaylistName => "Playlist name is invalid.",
            LocalizationKeys.RemoveSongFromPlaylistCommandInvalidTrackNumber => $"Track number '{args[1]}' is invalid for playlist '{args[0]}'.",
            LocalizationKeys.RemoveSongFromPlaylistCommandUnknownError => $"Could not remove track {args[1]} from playlist '{args[0]}'.",
            LocalizationKeys.RenamePlaylistCommandRenamed => $"Playlist '{args[0]}' renamed to '{args[1]}'.",
            LocalizationKeys.RenamePlaylistCommandPlaylistDoesNotExist => $"Playlist '{args[0]}' does not exist.",
            LocalizationKeys.RenamePlaylistCommandPlaylistAlreadyExists => $"Playlist '{args[0]}' already exists.",
            LocalizationKeys.RenamePlaylistCommandInvalidPlaylistName => "Playlist name is invalid.",
            LocalizationKeys.RenamePlaylistCommandUnknownError => $"Could not rename playlist '{args[0]}' to '{args[1]}'.",
            LocalizationKeys.DeletePlaylistCommandDeleted => $"Playlist '{args[0]}' deleted.",
            LocalizationKeys.DeletePlaylistCommandDoesNotExist => $"Playlist '{args[0]}' does not exist.",
            LocalizationKeys.DeletePlaylistCommandInvalidPlaylistName => "Playlist name is invalid.",
            LocalizationKeys.DeletePlaylistCommandConfirmationRequired => $"Set confirm to true to delete playlist '{args[0]}'.",
            LocalizationKeys.DeletePlaylistCommandUnknownError => $"Could not delete playlist '{args[0]}'.",
            LocalizationKeys.SlashCommandGuildOnly => "This command can only be used in a server.",
            LocalizationKeys.SlashCommandDeferredAccepted => "Request accepted.",
            LocalizationKeys.SlashCommandNotRegistered => $"Command '{args[0]}' is not registered.",
            LocalizationKeys.SlashCommandUnexpectedError => "An unexpected error occurred while executing the command.",
            ValidationErrorKeys.UserNotInVoiceChannel => "You must be in a voice channel.",
            _ => key
        };
    }
}
