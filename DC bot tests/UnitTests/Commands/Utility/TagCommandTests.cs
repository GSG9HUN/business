using DC_bot.Commands.Utility;
using DC_bot.Constants;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.Utility;

public class TagCommandTests
{
    private const string TagCommandName = "tag";
    private const string TagCommandDescriptionValue = "You can tag someone.";
    private const string TagCommandUsageContent = "!tag";
    private const string TagCommandContentUser = "!tag TestUser";
    private const string TagCommandContentInvalid = "!tag test";
    private const string TestUserName = "TestUser";
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

        _localizationServiceMock.Setup(g => g.Get(LocalizationKeys.TagCommandUserNotExistError, TestUserLower))
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
                var parts = (msg.Content ?? string.Empty).Split(" ", 2);
                return Task.FromResult(parts.Length < 2 ? null : parts[1].Trim());
            });

        var userValidationService = new ValidationService(validationLoggerMock.Object);
        _tagCommand = new TagCommand(userValidationService, logger.Object, _responseBuilderMock.Object,
            _localizationServiceMock.Object, _commandHelperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_User_Not_Provided_Username()
    {
        //Arrange
        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns(TagCommandUsageContent);

        //Act
        await _tagCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _responseBuilderMock.Verify(r => r.SendUsageAsync(_messageMock.Object, TagCommandName), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Username()
    {
        //Arrange
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

        //Act
        await _tagCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _responseBuilderMock.Verify(r => r.SendSuccessAsync(_messageMock.Object,
                $"{_localizationServiceMock.Object.Get(LocalizationKeys.TagCommandResponse, discordMemberMock.Object.Mention)}"),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Wrong_Username()
    {
        //Arrange
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

        //Act
        await _tagCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _responseBuilderMock.Verify(
            r => r.SendSuccessAsync(_messageMock.Object,
                $"{_localizationServiceMock.Object.Get(LocalizationKeys.TagCommandUserNotExistError, TestUserLower)}"),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsBot_ShouldDoNothing()
    {
        //Arrange
        _discordUserMock.SetupGet(du => du.IsBot).Returns(true);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

        //Act
        await _tagCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _messageMock.Verify(m => m.Channel.SendMessageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal(TagCommandName, _tagCommand.Name);
        Assert.Equal(TagCommandDescriptionValue, _tagCommand.Description);
    }
}