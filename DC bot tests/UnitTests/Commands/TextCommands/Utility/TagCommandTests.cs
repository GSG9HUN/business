using DC_bot.Commands.TextCommands.Utility;
using DC_bot.Constants;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Utility;

[Trait("Category", "Unit")]
public class TagCommandTests
{
    private const string TagCommandName = "tag";
    private const string TagCommandDescriptionValue = "You can tag someone.";
    private const string TagCommandUsageContent = "!tag";
    private const string TagCommandContentUser = "!tag TestUser";
    private const string TagCommandContentMention = "!tag <@123456789>";
    private const string TagCommandContentInvalid = "!tag test";
    private const string TestUserName = "TestUser";
    private const string TestUserMention = "<@123456789>";
    private const string TestUserLower = "test";
    private const string TagCommandDescriptionSetupValue = "You can tag someone.";
    private const string TagCommandResponseValue = "Tagged: TestUser";
    private const string TagCommandUserNotFoundValue = "User test not found.";
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<ICommandHelper> _commandHelperMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();

    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;
    private readonly TagCommand _tagCommand;

    public TagCommandTests()
    {
        Mock<ILogger<ValidationService>> validationLoggerMock = new();

        _localizationServiceMock.Setup(g => g.Get(LocalizationKeys.TagCommandDescription))
            .Returns(TagCommandDescriptionSetupValue);

        _localizationServiceMock.Setup(g => g.Get(LocalizationKeys.TagCommandResponse, TestUserName))
            .Returns(TagCommandResponseValue);
        _localizationServiceMock.Setup(g => g.Get(It.IsAny<ulong>(), LocalizationKeys.TagCommandResponse, TestUserName))
            .Returns(TagCommandResponseValue);

        _localizationServiceMock.Setup(g => g.Get(LocalizationKeys.TagCommandUserNotExistError, TestUserLower))
            .Returns(TagCommandUserNotFoundValue);
        _localizationServiceMock
            .Setup(g => g.Get(It.IsAny<ulong>(), LocalizationKeys.TagCommandUserNotExistError, TestUserLower))
            .Returns(TagCommandUserNotFoundValue);

        var logger = new Mock<ILogger<TagCommand>>();

        _messageMock = new Mock<IDiscordMessage>();
        _channelMock = new Mock<IDiscordChannel>();
        _guildMock = new Mock<IDiscordGuild>();
        _discordUserMock = new Mock<IDiscordUser>();
        _responseBuilderMock = new Mock<IResponseBuilder>();
        _commandHelperMock = new Mock<ICommandHelper>();
        _commandHelperMock
            .Setup(h => h.TryGetArgumentAsync(
                It.IsAny<IDiscordMessage>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<ILogger>(),
                It.IsAny<string>()))
            .Returns<IDiscordMessage, IResponseBuilder, ILogger, string>((msg, _, _, _) =>
            {
                var parts = msg.Content.Split(" ", 2);
                return Task.FromResult(parts.Length < 2 ? null : parts[1].Trim());
            });

        var userValidationService = new ValidationService(validationLoggerMock.Object);
        _tagCommand = new TagCommand(userValidationService, logger.Object, _responseBuilderMock.Object,
            _localizationServiceMock.Object, _commandHelperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_User_Not_Provided_Username()
    {
        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns(TagCommandUsageContent);

        await _tagCommand.ExecuteAsync(_messageMock.Object);

        _responseBuilderMock.Verify(r => r.SendUsageAsync(_messageMock.Object, TagCommandName), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserProvidedWhitespaceOnlyArgument_ShouldSendUsageAndStop()
    {
        _commandHelperMock
            .Setup(h => h.TryGetArgumentAsync(
                It.IsAny<IDiscordMessage>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<ILogger>(),
                It.IsAny<string>()))
            .Returns<IDiscordMessage, IResponseBuilder, ILogger, string>(async (msg, rb, _, commandName) =>
            {
                var parts = msg.Content.Split(" ", 2);
                if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1])) return parts[1].Trim();

                await rb.SendUsageAsync(msg, commandName);
                return null;
            });

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag   ");

        await _tagCommand.ExecuteAsync(_messageMock.Object);

        _responseBuilderMock.Verify(r => r.SendUsageAsync(_messageMock.Object, TagCommandName), Times.Once);
        _guildMock.Verify(g => g.GetAllMembersAsync(), Times.Never);
        _responseBuilderMock.Verify(
            r => r.SendSuccessAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>(), It.IsAny<object[]>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Username()
    {
        var discordMemberMock = new Mock<IDiscordMember>();
        discordMemberMock.SetupGet(dm => dm.Id).Returns(123456789UL);
        discordMemberMock.SetupGet(dm => dm.Username).Returns(TestUserName);
        discordMemberMock.SetupGet(dm => dm.Mention).Returns(TestUserName);

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _guildMock.Setup(g => g.GetAllMembersAsync())
            .ReturnsAsync(new List<IDiscordMember> { discordMemberMock.Object });
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns(TagCommandContentUser);

        await _tagCommand.ExecuteAsync(_messageMock.Object);

        _responseBuilderMock.Verify(r => r.SendSuccessAsync(
                _messageMock.Object,
                LocalizationKeys.TagCommandResponse,
                It.Is<object[]>(args => args.Length == 1 && (string)args[0] == discordMemberMock.Object.Mention)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Mention()
    {
        var discordMemberMock = new Mock<IDiscordMember>();
        discordMemberMock.SetupGet(dm => dm.Id).Returns(123456789UL);
        discordMemberMock.SetupGet(dm => dm.Username).Returns(TestUserName);
        discordMemberMock.SetupGet(dm => dm.Mention).Returns(TestUserMention);

        _localizationServiceMock.Setup(g => g.Get(LocalizationKeys.TagCommandResponse, TestUserMention))
            .Returns($"Tagged: {TestUserMention}");
        _localizationServiceMock.Setup(g => g.Get(It.IsAny<ulong>(), LocalizationKeys.TagCommandResponse, TestUserMention))
            .Returns($"Tagged: {TestUserMention}");

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _guildMock.Setup(g => g.GetAllMembersAsync())
            .ReturnsAsync(new List<IDiscordMember> { discordMemberMock.Object });
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns(TagCommandContentMention);

        await _tagCommand.ExecuteAsync(_messageMock.Object);

        _responseBuilderMock.Verify(r => r.SendSuccessAsync(
                _messageMock.Object,
                LocalizationKeys.TagCommandResponse,
                It.Is<object[]>(args => args.Length == 1 && (string)args[0] == discordMemberMock.Object.Mention)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Wrong_Username()
    {
        var discordMemberMock = new Mock<IDiscordMember>();
        discordMemberMock.SetupGet(dm => dm.Id).Returns(123456789UL);
        discordMemberMock.SetupGet(dm => dm.Username).Returns(TestUserName);
        discordMemberMock.SetupGet(dm => dm.Mention).Returns(TestUserName);

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _guildMock.Setup(g => g.GetAllMembersAsync())
            .ReturnsAsync(new List<IDiscordMember> { discordMemberMock.Object });
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns(TagCommandContentInvalid);

        await _tagCommand.ExecuteAsync(_messageMock.Object);

        _responseBuilderMock.Verify(
            r => r.SendWarningAsync(_messageMock.Object,
                LocalizationKeys.TagCommandUserNotExistError,
                It.Is<object[]>(args => args.Length == 1 && (string)args[0] == TestUserLower)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsBot_ShouldDoNothing()
    {
        _discordUserMock.SetupGet(du => du.IsBot).Returns(true);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

        await _tagCommand.ExecuteAsync(_messageMock.Object);

        _messageMock.Verify(m => m.Channel.SendMessageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal(TagCommandName, _tagCommand.Name);
        Assert.Equal(TagCommandDescriptionValue, _tagCommand.Description);
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Username_CaseInsensitive()
    {
        var discordMemberMock = new Mock<IDiscordMember>();
        discordMemberMock.SetupGet(dm => dm.Id).Returns(123456789UL);
        discordMemberMock.SetupGet(dm => dm.Username).Returns("TestUser");
        discordMemberMock.SetupGet(dm => dm.Mention).Returns("TestUser");

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _guildMock.Setup(g => g.GetAllMembersAsync())
            .ReturnsAsync(new List<IDiscordMember> { discordMemberMock.Object });
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag TESTUSER");

        await _tagCommand.ExecuteAsync(_messageMock.Object);

        _responseBuilderMock.Verify(r => r.SendSuccessAsync(
                _messageMock.Object,
                LocalizationKeys.TagCommandResponse,
                It.Is<object[]>(args => args.Length == 1 && (string)args[0] == discordMemberMock.Object.Mention)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Username_WithWhitespace()
    {
        var discordMemberMock = new Mock<IDiscordMember>();
        discordMemberMock.SetupGet(dm => dm.Id).Returns(123456789UL);
        discordMemberMock.SetupGet(dm => dm.Username).Returns("TestUser");
        discordMemberMock.SetupGet(dm => dm.Mention).Returns("TestUser");

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _guildMock.Setup(g => g.GetAllMembersAsync())
            .ReturnsAsync(new List<IDiscordMember> { discordMemberMock.Object });
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag   TestUser   ");

        await _tagCommand.ExecuteAsync(_messageMock.Object);

        _responseBuilderMock.Verify(r => r.SendSuccessAsync(
                _messageMock.Object,
                LocalizationKeys.TagCommandResponse,
                It.Is<object[]>(args => args.Length == 1 && (string)args[0] == discordMemberMock.Object.Mention)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Username_NotFound_EmptyMemberList()
    {
        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _guildMock.Setup(g => g.GetAllMembersAsync())
            .ReturnsAsync(new List<IDiscordMember>());
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag test");

        await _tagCommand.ExecuteAsync(_messageMock.Object);

        _responseBuilderMock.Verify(
            r => r.SendWarningAsync(_messageMock.Object,
                LocalizationKeys.TagCommandUserNotExistError,
                It.Is<object[]>(args => args.Length == 1 && (string)args[0] == "test")),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_TryGetArgumentAsync_Throws_ShouldPropagate()
    {
        var commandHelperMock = new Mock<ICommandHelper>();
        commandHelperMock
            .Setup(h => h.TryGetArgumentAsync(
                It.IsAny<IDiscordMessage>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<ILogger>(),
                It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Helper failed"));

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag TestUser");

        var userValidationService = new ValidationService(new Mock<ILogger<ValidationService>>().Object);
        var tagCommand = new TagCommand(userValidationService, new Mock<ILogger<TagCommand>>().Object,
            _responseBuilderMock.Object, _localizationServiceMock.Object, commandHelperMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => tagCommand.ExecuteAsync(_messageMock.Object));
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Username_NullGuild()
    {
        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _channelMock.SetupGet(c => c.Guild).Returns((IDiscordGuild)null!);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag TestUser");

        await Assert.ThrowsAsync<NullReferenceException>(() => _tagCommand.ExecuteAsync(_messageMock.Object));
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Username_NullMembers()
    {
        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _guildMock.Setup(g => g.GetAllMembersAsync())
            .ReturnsAsync((List<IDiscordMember>)null!);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag TestUser");

        await Assert.ThrowsAsync<ArgumentNullException>(() => _tagCommand.ExecuteAsync(_messageMock.Object));
    }

    [Fact]
    public async Task ExecuteAsync_GetAllMembersAsync_Throws_ShouldPropagate()
    {
        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _guildMock.Setup(g => g.GetAllMembersAsync())
            .ThrowsAsync(new InvalidOperationException("Guild unavailable"));
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag TestUser");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _tagCommand.ExecuteAsync(_messageMock.Object));
    }

    [Fact]
    public async Task ExecuteAsync_Logs_CommandInvoked_And_CommandExecuted()
    {
        var loggerMock = new Mock<ILogger<TagCommand>>();
        var discordMemberMock = new Mock<IDiscordMember>();
        discordMemberMock.SetupGet(dm => dm.Id).Returns(123456789UL);
        discordMemberMock.SetupGet(dm => dm.Username).Returns("TestUser");
        discordMemberMock.SetupGet(dm => dm.Mention).Returns("TestUser");

        loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _guildMock.Setup(g => g.GetAllMembersAsync())
            .ReturnsAsync(new List<IDiscordMember> { discordMemberMock.Object });
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag TestUser");

        var userValidationService = new ValidationService(new Mock<ILogger<ValidationService>>().Object);
        var tagCommand = new TagCommand(userValidationService, loggerMock.Object, _responseBuilderMock.Object,
            _localizationServiceMock.Object, _commandHelperMock.Object);

        await tagCommand.ExecuteAsync(_messageMock.Object);


        loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Debug),
                It.Is<EventId>(e => e.Id == 1001),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Command invoked: tag")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Debug),
                It.Is<EventId>(e => e.Id == 1002),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Command executed: tag")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
