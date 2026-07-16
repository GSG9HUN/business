using DSharpPlus.Entities;

namespace DC_bot_tests.EndToEndTests.Service;

internal sealed class LiveDiscordMessageProbe(
    DiscordChannel textChannel,
    DiscordMessage testRunMarker)
{
    public async Task<DiscordMessage> WaitForMusicControlMessageAsync()
    {
        return await AsyncTestWaiter.UntilNotNullAsync(
            async () =>
            {
                var messages = await textChannel.GetMessagesAfterAsync(testRunMarker.Id, 20);
                return messages
                    .OrderBy(message => message.CreationTimestamp)
                    .FirstOrDefault(message => message.Embeds.Count > 0);
            },
            "Music flow E2E did not publish a now-playing control message to Discord chat.");
    }

    public async Task<DiscordMessage> WaitForControlMessageDescriptionChangeAsync(
        ulong messageId,
        string initialDescription)
    {
        return await AsyncTestWaiter.UntilNotNullAsync(
            async () =>
            {
                var updatedMessage = await textChannel.GetMessageAsync(messageId);
                var updatedDescription = updatedMessage.Embeds.FirstOrDefault()?.Description ?? string.Empty;
                return string.Equals(updatedDescription, initialDescription, StringComparison.Ordinal)
                    ? null
                    : updatedMessage;
            },
            "Music flow E2E control message progress did not update in time.");
    }
}
