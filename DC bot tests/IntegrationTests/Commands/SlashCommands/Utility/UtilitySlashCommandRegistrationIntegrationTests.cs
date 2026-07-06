using DC_bot.Commands.SlashCommands.Utility;

namespace DC_bot_tests.IntegrationTests.Commands.SlashCommands.Utility;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class UtilitySlashCommandRegistrationIntegrationTests : SlashCommandRegistrationIntegrationTestBase
{
    [Fact]
    public async Task Create_ShouldRegisterUtilitySlashCommandModules()
    {
        await WithServiceProviderAsync(services => services.AssertResolvesRequiredServices(
            typeof(PingSlashCommand),
            typeof(HelpSlashCommand),
            typeof(TagSlashCommand),
            typeof(LanguageSlashCommand)));
    }
}
