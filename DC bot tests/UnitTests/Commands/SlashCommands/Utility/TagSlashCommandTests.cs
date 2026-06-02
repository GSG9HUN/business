using DC_bot.Commands.SlashCommands.Utility;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.SlashCommands.Utility;

[Trait("Category", "Unit")]
public class TagSlashCommandTests : SlashCommandTestBase
{
    [Fact]
    public void Tag_ShouldExposeMemberSlashOption()
    {
        var parameter = typeof(TagSlashCommand)
            .GetMethod(nameof(TagSlashCommand.Tag))!
            .GetParameters()[1];
        var parameterAttribute = Assert.Single(
            parameter.GetCustomAttributes(typeof(ParameterAttribute), inherit: false).Cast<ParameterAttribute>());

        Assert.Equal(typeof(DiscordMember), parameter.ParameterType);
        Assert.Equal("user", parameterAttribute.Name);
    }

    [Fact]
    public async Task Tag_ShouldCreateInteractionContextAndDelegateMemberMentionToExecutor()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var discordMember = CreateDiscordMember(999UL);
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new TagSlashCommand(executor.Object, contextFactory.Object);

        await command.Tag(dsharpContext, discordMember);

        contextFactory.Verify(x => x.Create(dsharpContext), Times.Once);
        VerifyRequest(executor, "tag", slashContext.Object, DiscordMemberMention, requireGuild: true, defer: true);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMemberProvided_ShouldDelegateMemberMentionToExecutor()
    {
        var slashContext = new Mock<ISlashInteractionContext>();
        var member = new Mock<IDiscordMember>();
        member.SetupGet(x => x.Mention).Returns(MemberMention);
        var executor = CreateModuleExecutor();
        var command = new TagSlashCommand(executor.Object, Mock.Of<ISlashInteractionContextFactory>());

        await command.ExecuteAsync(slashContext.Object, member.Object);

        VerifyRequest(executor, "tag", slashContext.Object, MemberMention, requireGuild: true, defer: true);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMemberMentionExists_ShouldTagMemberAfterDeferring()
    {
        var taggedMember = CreateMember("TestUser", "<@999>");
        var context = CreateContext(allMembers: [taggedMember]);

        await ExecuteSlashAsync(SlashCommandExecutor, "tag", context, "<@999>", requireGuild: true, defer: true);

        Assert.True(context.IsDeferred);
        Assert.Contains("Tagged: <@999>", context.TextResponses);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMemberDoesNotExist_ShouldReturnNotFoundAfterDeferring()
    {
        var context = CreateContext(allMembers: []);

        await ExecuteSlashAsync(SlashCommandExecutor, "tag", context, "MissingUser", requireGuild: true, defer: true);

        Assert.True(context.IsDeferred);
        Assert.Contains("User MissingUser not found.", context.TextResponses);
    }
}
