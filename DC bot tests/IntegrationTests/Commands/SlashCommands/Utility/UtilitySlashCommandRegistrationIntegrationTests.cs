using DC_bot.Commands.SlashCommands.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Commands.SlashCommands.Utility;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class UtilitySlashCommandRegistrationIntegrationTests : SlashCommandRegistrationIntegrationTestBase
{
    [Fact]
    public async Task Create_ShouldRegisterUtilitySlashCommandModules()
    {
        await WithServiceProviderAsync(services =>
        {
            Assert.NotNull(services.GetRequiredService<PingSlashCommand>());
            Assert.NotNull(services.GetRequiredService<HelpSlashCommand>());
            Assert.NotNull(services.GetRequiredService<TagSlashCommand>());
            Assert.NotNull(services.GetRequiredService<LanguageSlashCommand>());
        });
    }
}
