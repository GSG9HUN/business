using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using DC_bot.Wrapper;
using DSharpPlus.Entities;
using Moq;

namespace DC_bot_tests.TestHelperFiles;

public class TestSlashInteractionContext(
    IDiscordChannel? channel = null,
    IDiscordUser? user = null,
    IDiscordGuild? guild = null,
    IDiscordMember? member = null)
    : ISlashInteractionContext
{
    public List<string> TextResponses { get; } = [];
    public List<DiscordEmbed> EmbedResponses { get; } = [];
    public ulong? GuildId { get; } = guild?.Id;
    public IDiscordGuild? Guild { get; } = guild;
    public IDiscordChannel Channel { get; } = channel ?? CreateChannel(guild);
    public IDiscordUser User { get; } = user ?? CreateUser();
    public IDiscordMember? Member { get; } = member;
    public bool IsDeferred { get; private set; }
    public bool HasResponded => TextResponses.Count > 0 || EmbedResponses.Count > 0;

    public Task DeferAsync()
    {
        IsDeferred = true;
        return Task.CompletedTask;
    }

    public Task RespondAsync(string message)
    {
        TextResponses.Add(message);
        return Task.CompletedTask;
    }

    public Task RespondAsync(DiscordEmbed embed)
    {
        EmbedResponses.Add(embed);
        return Task.CompletedTask;
    }

    public IDiscordMessage CreateMessage(string commandName, string? argument = null)
    {
        return new SlashInteractionMessageWrapper(
            string.IsNullOrWhiteSpace(argument) ? commandName : $"{commandName} {argument}",
            this);
    }

    private static IDiscordChannel CreateChannel(IDiscordGuild? guild)
    {
        var channel = new Mock<IDiscordChannel>();
        channel.SetupGet(x => x.Id).Returns(456UL);
        channel.SetupGet(x => x.Name).Returns("slash-test");
        channel.SetupGet(x => x.Guild).Returns(guild!);
        return channel.Object;
    }

    private static IDiscordUser CreateUser()
    {
        var user = new Mock<IDiscordUser>();
        user.SetupGet(x => x.Id).Returns(123UL);
        user.SetupGet(x => x.Username).Returns("SlashUser");
        user.SetupGet(x => x.Mention).Returns("<@123>");
        user.SetupGet(x => x.IsBot).Returns(false);
        return user.Object;
    }
}
