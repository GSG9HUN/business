using DC_bot.Commands.SlashCommands.Music;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Commands.SlashCommands.Music;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class MusicSlashCommandRegistrationIntegrationTests : SlashCommandRegistrationIntegrationTestBase
{
    [Fact]
    public async Task Create_ShouldRegisterMusicSlashCommandModules()
    {
        await WithServiceProviderAsync(services =>
        {
            Assert.NotNull(services.GetRequiredService<JoinSlashCommand>());
            Assert.NotNull(services.GetRequiredService<PlaySlashCommand>());
            Assert.NotNull(services.GetRequiredService<SkipSlashCommand>());
            Assert.NotNull(services.GetRequiredService<PauseSlashCommand>());
            Assert.NotNull(services.GetRequiredService<ResumeSlashCommand>());
            Assert.NotNull(services.GetRequiredService<LeaveSlashCommand>());
        });
    }
}
