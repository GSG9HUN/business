using System.Reflection;
using System.Runtime.CompilerServices;
using DC_bot.Commands.TextCommands.Playlist;
using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using DC_bot.Service.Presentation;
using DC_bot_tests.TestHelperFiles;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.EndToEndTests.Commands.TextCommands.Playlist;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class PlaylistTextCommandEndToEndTests
{
    private const ulong GuildId = 456ul;
    private const string PlaylistName = "e2e";
    private const string RenamedPlaylistName = "renamed-e2e";

    [Fact]
    public async Task PlaylistTextCommands_RouteThroughCommandHandlerAndReturnLocalizedResponses()
    {
        var responses = new List<string>();
        var localizationService = CreateLocalizationService();
        var playlistService = new Mock<IPlaylistService>();
        playlistService.Setup(service => service.CreatePlaylistAsync(GuildId, PlaylistName))
            .ReturnsAsync(CreatePlaylistResult.Created);
        playlistService.Setup(service => service.ListPlaylistsAsync(GuildId))
            .ReturnsAsync(new ListPlaylistsResult(
                ListPlaylistsStatus.Listed,
                [new PlaylistSummaryDto(PlaylistName, 1)]));
        playlistService.Setup(service => service.ViewPlaylistAsync(GuildId, PlaylistName))
            .ReturnsAsync(new ViewPlaylistResult(
                ViewPlaylistStatus.Viewed,
                PlaylistName,
                [new PlaylistViewTrackDto(1, "Song", "Artist", TimeSpan.FromSeconds(95), "https://example.com/song")]));
        playlistService.Setup(service => service.RemoveSongFromPlaylistAsync(GuildId, PlaylistName, 1))
            .ReturnsAsync(RemoveSongResult.Removed);
        playlistService.Setup(service => service.RenamePlaylistAsync(GuildId, PlaylistName, RenamedPlaylistName))
            .ReturnsAsync(RenamePlaylistResult.Renamed);
        playlistService.Setup(service => service.DeletePlaylistAsync(GuildId, RenamedPlaylistName))
            .ReturnsAsync(DeletePlaylistResult.Deleted);

        await using var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(localizationService)
            .AddSingleton<IResponseBuilder, ResponseBuilder>()
            .AddSingleton<IUserValidationService>(_ => new ValidationService(Mock.Of<ILogger<ValidationService>>()))
            .AddSingleton<ICommandHelper, CommandValidationService>()
            .AddSingleton(playlistService.Object)
            .AddSingleton<Func<IEnumerable<ICommand>>>(provider => () => provider.GetServices<ICommand>())
            .AddSingleton<ICommandRegistry, CommandRegistry>()
            .AddSingleton<ICommand, CreatePlaylistCommand>()
            .AddSingleton<ICommand, ListPlaylistsCommand>()
            .AddSingleton<ICommand, ViewPlaylistCommand>()
            .AddSingleton<ICommand, RemoveSongFromPlaylistCommand>()
            .AddSingleton<ICommand, RenamePlaylistCommand>()
            .AddSingleton<ICommand, DeletePlaylistCommand>()
            .BuildServiceProvider();

        var handler = new CommandHandlerService(
            services.GetRequiredService<ICommandRegistry>(),
            Mock.Of<ILogger<CommandHandlerService>>(),
            localizationService,
            new BotSettings { Prefix = "!" },
            isTestMode: true,
            new CapturingDiscordMessageFactory(responses));
        var client = DiscordClientBuilder
            .CreateDefault("fake-token", DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents)
            .Build();

        try
        {
            handler.RegisterHandler(client);

            await handler.HandleEventAsync(client, CreateMessageCreated("!createPlaylist e2e"));
            await handler.HandleEventAsync(client, CreateMessageCreated("!listPlaylists"));
            await handler.HandleEventAsync(client, CreateMessageCreated("!viewPlaylist e2e"));
            await handler.HandleEventAsync(client, CreateMessageCreated("!removeSong e2e 1"));
            await handler.HandleEventAsync(client, CreateMessageCreated("!renamePlaylist e2e renamed-e2e"));
            await handler.HandleEventAsync(client, CreateMessageCreated("!deletePlaylist renamed-e2e"));
        }
        finally
        {
            handler.UnregisterHandler(client);
            DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(client);
        }

        Assert.Contains("Playlist 'e2e' created.", responses);
        Assert.Contains(responses, response =>
            response.Contains("Saved playlists:", StringComparison.Ordinal) &&
            response.Contains("1. e2e - 1 tracks", StringComparison.Ordinal));
        Assert.Contains(responses, response =>
            response.Contains("Playlist 'e2e' (1 tracks):", StringComparison.Ordinal) &&
            response.Contains("1. Artist - Song (1:35)", StringComparison.Ordinal));
        Assert.Contains("Track 1 removed from playlist 'e2e'.", responses);
        Assert.Contains("Playlist 'e2e' renamed to 'renamed-e2e'.", responses);
        Assert.Contains("Playlist 'renamed-e2e' deleted.", responses);
    }

    private static ILocalizationService CreateLocalizationService()
    {
        var localization = new Mock<ILocalizationService>();
        localization.Setup(service => service.Get(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<string, object[]>(FormatLocalization);
        localization.Setup(service => service.Get(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<ulong, string, object[]>((_, key, args) => FormatLocalization(key, args));
        return localization.Object;
    }

    private static string FormatLocalization(string key, object[] args)
    {
        return key switch
        {
            "response_warning_prefix" => "**Warning:** ",
            "response_error_prefix" => "**Error:** ",
            LocalizationKeys.CreatePlaylistCommandCreated => $"Playlist '{args[0]}' created.",
            LocalizationKeys.ListPlaylistsCommandResponse => $"Saved playlists:{Environment.NewLine}{args[0]}",
            LocalizationKeys.ListPlaylistsCommandItem => $"{args[0]}. {args[1]} - {args[2]} tracks",
            LocalizationKeys.ViewPlaylistCommandResponse => $"Playlist '{args[0]}' ({args[1]} tracks):{Environment.NewLine}{args[2]}",
            LocalizationKeys.ViewPlaylistCommandTrack => $"{args[0]}. {args[1]} - {args[2]} ({args[3]})",
            LocalizationKeys.ViewPlaylistCommandMoreTracks => $"... and {args[0]} more tracks",
            LocalizationKeys.RemoveSongFromPlaylistCommandRemoved => $"Track {args[1]} removed from playlist '{args[0]}'.",
            LocalizationKeys.RenamePlaylistCommandRenamed => $"Playlist '{args[0]}' renamed to '{args[1]}'.",
            LocalizationKeys.DeletePlaylistCommandDeleted => $"Playlist '{args[0]}' deleted.",
            _ => args.Length == 0 ? key : $"{key}:{string.Join("|", args)}"
        };
    }

    private static MessageCreatedEventArgs CreateMessageCreated(string content)
    {
        var author = Create<DiscordUser>();
        SetMember(author, "Id", 123ul);
        SetMember(author, "Username", "PlaylistE2EUser");
        SetMember(author, "IsBot", false);

        var guild = Create<DiscordGuild>();
        SetMember(guild, "Id", GuildId);
        SetMember(guild, "Name", "Playlist E2E Guild");

        var channel = Create<DiscordChannel>();
        SetMember(channel, "Id", 789ul);
        SetMember(channel, "Name", "playlist-e2e-channel");
        TrySetMember(channel, "Guild", guild);

        var message = Create<DiscordMessage>();
        SetMember(message, "Id", (ulong)Random.Shared.Next(1, int.MaxValue));
        SetMember(message, "Content", content);
        SetMember(message, "embeds", new List<DiscordEmbed>());
        TrySetMember(message, "Author", author);
        TrySetMember(message, "Channel", channel);

        var args = Create<MessageCreatedEventArgs>();
        SetMember(args, "Message", message);
        TrySetMember(args, "Author", author);
        TrySetMember(args, "Channel", channel);
        TrySetMember(args, "Guild", guild);

        return args;
    }

    private static T Create<T>() => (T)RuntimeHelpers.GetUninitializedObject(typeof(T));

    private static bool TrySetMember(object obj, string name, object? value)
    {
        var type = obj.GetType();
        while (type is not null)
        {
            var backingField = type.GetField($"<{name}>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (backingField is not null)
            {
                backingField.SetValue(obj, value);
                return true;
            }

            var underscored = type.GetField($"_{char.ToLowerInvariant(name[0])}{name[1..]}",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (underscored is not null)
            {
                underscored.SetValue(obj, value);
                return true;
            }

            var matchingField = type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(field =>
                    string.Equals(field.Name, name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(field.Name.TrimStart('_'), name, StringComparison.OrdinalIgnoreCase));
            if (matchingField is not null)
            {
                matchingField.SetValue(obj, value);
                return true;
            }

            var property = type.GetProperty(name,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (property?.SetMethod is not null)
            {
                property.SetValue(obj, value);
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    private static void SetMember(object obj, string name, object? value)
    {
        if (!TrySetMember(obj, name, value))
        {
            throw new InvalidOperationException($"Member '{name}' not found on {obj.GetType().Name}.");
        }
    }

    private sealed class CapturingDiscordMessageFactory(List<string> responses) : IDiscordMessageFactory
    {
        public IDiscordMessage Create(
            DiscordMessage message,
            DiscordChannel channel,
            DiscordUser author,
            DiscordGuild? guild = null)
        {
            var voiceChannel = new Mock<IDiscordChannel>();
            voiceChannel.SetupGet(item => item.Id).Returns(321ul);
            voiceChannel.SetupGet(item => item.Name).Returns("voice");

            var voiceState = new Mock<IDiscordVoiceState>();
            voiceState.SetupGet(item => item.Channel).Returns(voiceChannel.Object);

            var member = new Mock<IDiscordMember>();
            member.SetupGet(item => item.Id).Returns(author.Id);
            member.SetupGet(item => item.Username).Returns("PlaylistE2EUser");
            member.SetupGet(item => item.IsBot).Returns(false);
            member.SetupGet(item => item.VoiceState).Returns(voiceState.Object);

            var discordGuild = new Mock<IDiscordGuild>();
            discordGuild.SetupGet(item => item.Id).Returns(GuildId);
            discordGuild.SetupGet(item => item.Name).Returns("Playlist E2E Guild");
            discordGuild.Setup(item => item.GetMemberAsync(author.Id)).ReturnsAsync(member.Object);

            var discordChannel = new Mock<IDiscordChannel>();
            discordChannel.SetupGet(item => item.Id).Returns(channel.Id);
            discordChannel.SetupGet(item => item.Name).Returns("playlist-e2e-channel");
            discordChannel.SetupGet(item => item.Guild).Returns(discordGuild.Object);

            var discordUser = new Mock<IDiscordUser>();
            discordUser.SetupGet(item => item.Id).Returns(author.Id);
            discordUser.SetupGet(item => item.Username).Returns("PlaylistE2EUser");
            discordUser.SetupGet(item => item.IsBot).Returns(false);

            var discordMessage = new Mock<IDiscordMessage>();
            discordMessage.SetupGet(item => item.Id).Returns(message.Id);
            discordMessage.SetupGet(item => item.Content).Returns(message.Content);
            discordMessage.SetupGet(item => item.Author).Returns(discordUser.Object);
            discordMessage.SetupGet(item => item.Channel).Returns(discordChannel.Object);
            discordMessage.Setup(item => item.RespondAsync(It.IsAny<string>()))
                .Callback<string>(responses.Add)
                .Returns(Task.CompletedTask);

            return discordMessage.Object;
        }
    }
}
