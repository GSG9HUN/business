using DC_bot.Configuration;
using DC_bot.Interface;
using DC_bot.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Commands.TextCommands;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class TextCommandRegistrationIntegrationTests
{
    [Fact]
    public async Task Create_ShouldResolveEveryTextCommandFromStartupGraph()
    {
        var provider = BotServiceProviderFactory.Create(
            new BotSettings { Token = "fake-token", Prefix = "!" },
            new LavalinkSettings
            {
                Hostname = "localhost",
                Port = 2333,
                Password = "password",
                Secured = false
            },
            "Host=localhost;Port=5432;Database=bot_tests;Username=postgres;Password=postgres");

        try
        {
            var commands = provider.GetServices<ICommand>().ToArray();

            Assert.Equal(23, commands.Length);
            Assert.Equal(
                [
                    "tag",
                    "join",
                    "ping",
                    "help",
                    "play",
                    "skip",
                    "clear",
                    "leave",
                    "pause",
                    "resume",
                    "repeat",
                    "shuffle",
                    "language",
                    "viewList",
                    "createPlaylist",
                    "savePlaylist",
                    "deletePlaylist",
                    "addSong",
                    "removeSong",
                    "listPlaylists",
                    "viewPlaylist",
                    "renamePlaylist",
                    "repeatList"
                ],
                commands.Select(command => command.Name));
            Assert.Equal(commands.Length, commands.Select(command => command.GetType()).Distinct().Count());
        }
        finally
        {
            await ServiceProviderDisposeHelper.DisposeIgnoringDisconnectedDiscordClientAsync(provider);
        }
    }
}
