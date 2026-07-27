using DC_bot.Interface.Discord;
using DSharpPlus.Entities;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service.Core;

internal sealed class CommandHandlerFakeMessageFactory : IDiscordMessageFactory
{
    public IDiscordMessage Create(
        DiscordMessage message,
        DiscordChannel channel,
        DiscordUser author,
        DiscordGuild? guild = null)
    {
        var voiceState = new Mock<IDiscordVoiceState>();
        voiceState.SetupGet(x => x.Channel).Returns((IDiscordChannel?)null);

        var member = new Mock<IDiscordMember>();
        member.SetupGet(x => x.Id).Returns(author.Id);
        member.SetupGet(x => x.Username).Returns("IntegrationUser");
        member.SetupGet(x => x.Mention).Returns("<@123>");
        member.SetupGet(x => x.IsBot).Returns(author.IsBot);
        member.SetupGet(x => x.VoiceState).Returns(voiceState.Object);

        var discordGuild = new Mock<IDiscordGuild>();
        discordGuild.SetupGet(x => x.Id).Returns(456UL);
        discordGuild.SetupGet(x => x.Name).Returns("IntegrationGuild");
        discordGuild.Setup(x => x.GetMemberAsync(author.Id)).ReturnsAsync(member.Object);
        discordGuild.Setup(x => x.GetAllMembersAsync()).ReturnsAsync([member.Object]);

        var discordChannel = new Mock<IDiscordChannel>();
        discordChannel.SetupGet(x => x.Id).Returns(channel.Id);
        discordChannel.SetupGet(x => x.Name).Returns("integration-channel");
        discordChannel.SetupGet(x => x.Guild).Returns(discordGuild.Object);

        var discordUser = new Mock<IDiscordUser>();
        discordUser.SetupGet(x => x.Id).Returns(author.Id);
        discordUser.SetupGet(x => x.Username).Returns("IntegrationUser");
        discordUser.SetupGet(x => x.Mention).Returns("<@123>");
        discordUser.SetupGet(x => x.IsBot).Returns(author.IsBot);

        var discordMessage = new Mock<IDiscordMessage>();
        discordMessage.SetupGet(x => x.Id).Returns(message.Id);
        discordMessage.SetupGet(x => x.Content).Returns(message.Content);
        discordMessage.SetupGet(x => x.Author).Returns(discordUser.Object);
        discordMessage.SetupGet(x => x.Channel).Returns(discordChannel.Object);
        discordMessage.SetupGet(x => x.CreatedAt).Returns(message.CreationTimestamp);
        discordMessage.SetupGet(x => x.Embeds).Returns(message.Embeds.ToList());
        return discordMessage.Object;
    }
}
