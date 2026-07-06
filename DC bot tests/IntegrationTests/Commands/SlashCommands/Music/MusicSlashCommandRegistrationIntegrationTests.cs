using DC_bot.Commands.SlashCommands.Music;

namespace DC_bot_tests.IntegrationTests.Commands.SlashCommands.Music;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class MusicSlashCommandRegistrationIntegrationTests : SlashCommandRegistrationIntegrationTestBase
{
    [Fact]
    public async Task Create_ShouldRegisterMusicSlashCommandModules()
    {
        await WithServiceProviderAsync(services => services.AssertResolvesRequiredServices(
            typeof(JoinSlashCommand),
            typeof(PlaySlashCommand),
            typeof(SkipSlashCommand),
            typeof(PauseSlashCommand),
            typeof(ResumeSlashCommand),
            typeof(LeaveSlashCommand)));
    }
}
