using DC_bot.Commands.SlashCommands.Playlist;

namespace DC_bot_tests.IntegrationTests.Commands.SlashCommands.Playlist;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class PlaylistSlashCommandRegistrationIntegrationTests : SlashCommandRegistrationIntegrationTestBase
{
    [Fact]
    public async Task Create_ShouldRegisterPlaylistSlashCommandModule()
    {
        await WithServiceProviderAsync(services => services.AssertResolvesRequiredServices(
            typeof(PlaylistSlashCommand)));
    }
}
