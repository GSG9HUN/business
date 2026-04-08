using DC_bot.Commands.Utility;
using DC_bot.Constants;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
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

    [Fact]
    public async Task ExecuteAsync_User_Provided_Username_CaseInsensitive()
    {
        // Arrange
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

        // Act
        await _tagCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _responseBuilderMock.Verify(r => r.SendSuccessAsync(_messageMock.Object,
                $"{_localizationServiceMock.Object.Get(LocalizationKeys.TagCommandResponse, discordMemberMock.Object.Mention)}"),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Username_WithWhitespace()
    {
        // Arrange
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

        // Act
        await _tagCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _responseBuilderMock.Verify(r => r.SendSuccessAsync(_messageMock.Object,
                $"{_localizationServiceMock.Object.Get(LocalizationKeys.TagCommandResponse, discordMemberMock.Object.Mention)}"),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Username_NotFound_EmptyMemberList()
    {
        // Arrange
        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _guildMock.Setup(g => g.GetAllMembersAsync())
            .ReturnsAsync(new List<IDiscordMember>());
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag test");

        // Act
        await _tagCommand.ExecuteAsync(_messageMock.Object);
 
        // Assert
        _responseBuilderMock.Verify(
            r => r.SendSuccessAsync(_messageMock.Object,
                $"{_localizationServiceMock.Object.Get(LocalizationKeys.TagCommandUserNotExistError, "test")}"),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_TryGetArgumentAsync_Throws_ShouldPropagate()
    {
        // Arrange
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

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => tagCommand.ExecuteAsync(_messageMock.Object));
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Username_NullGuild()
    {
        // Arrange
        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _channelMock.SetupGet(c => c.Guild).Returns((IDiscordGuild)null!);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag TestUser");

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _tagCommand.ExecuteAsync(_messageMock.Object));
    }

    [Fact]
    public async Task ExecuteAsync_User_Provided_Username_NullMembers()
    {
        // Arrange
        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _guildMock.Setup(g => g.GetAllMembersAsync())
            .ReturnsAsync((List<IDiscordMember>)null!);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag TestUser");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _tagCommand.ExecuteAsync(_messageMock.Object));
    }

    [Fact]
    public async Task ExecuteAsync_GetAllMembersAsync_Throws_ShouldPropagate()
    {
        // Arrange
        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _guildMock.Setup(g => g.GetAllMembersAsync())
            .ThrowsAsync(new InvalidOperationException("Guild unavailable"));
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!tag TestUser");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _tagCommand.ExecuteAsync(_messageMock.Object));
    }

    [Fact]
    public async Task ExecuteAsync_Logs_CommandInvoked_And_CommandExecuted()
    {
        // Arrange
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

        // Act
        await tagCommand.ExecuteAsync(_messageMock.Object);

        // Assert

        loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.Is<EventId>(e => e.Id == 1001),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Command invoked: tag")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        
        loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.Is<EventId>(e => e.Id == 1002),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Command executed: tag")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}