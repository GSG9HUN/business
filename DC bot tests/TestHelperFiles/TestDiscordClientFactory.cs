using DSharpPlus;

namespace DC_bot_tests.TestHelperFiles;

public static class TestDiscordClientFactory
{
    public static DiscordClient Create(
        string token = "fake-test-token",
        DiscordIntents intents = DiscordIntents.AllUnprivileged)
    {
        return DiscordClientBuilder
            .CreateDefault(token, intents)
            .Build();
    }
}
