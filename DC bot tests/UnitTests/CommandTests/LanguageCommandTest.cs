using DC_bot.Commands;
using DC_bot.Interface;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class LanguageCommandTest
{
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly LanguageCommand _languageCommand;
    private readonly Mock<ILocalizationService> _localizationServiceMock;

    public LanguageCommandTest()
    {
        Mock<ILogger<LanguageCommand>> loggerMock = new();
        Mock<IDiscordChannel> channelMock = new();
        _localizationServiceMock = new Mock<ILocalizationService>();

        _localizationServiceMock.Setup(l => l.Get("language_command_response"))
            .Returns("The language changed successfully.");

        _localizationServiceMock.Setup(l => l.Get("language_command_usage"))
            .Returns("Please provide language.");

        _localizationServiceMock.Setup(l => l.Get("language_command_response"))
            .Returns("The language changed successfully.");
        
        _localizationServiceMock.Setup(l => l.Get("language_command_description"))
            .Returns("Change the bot language.");

        _messageMock = new Mock<IDiscordMessage>();
        _guildMock = new Mock<IDiscordGuild>();

        _messageMock.Setup(m => m.Channel).Returns(channelMock.Object);
        channelMock.Setup(c => c.Guild).Returns(_guildMock.Object);

        _languageCommand = new LanguageCommand(loggerMock.Object, _localizationServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_UsageMessage_When_No_Language_Provided()
    {
        // Arrange
        _messageMock.Setup(m => m.Content).Returns("!language");


        // Act
        await _languageCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _messageMock.Verify(m => m.RespondAsync("Please provide language."), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_SaveLanguage_And_Send_Response_When_Language_Is_Provided()
    {
        // Arrange
        ulong guildId = 123456;
        _messageMock.Setup(m => m.Content).Returns("!language hu");
        _guildMock.Setup(g => g.Id).Returns(guildId);

        // Act
        await _languageCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _localizationServiceMock.Verify(l => l.SaveLanguage(guildId, "hu"), Times.Once);
        _messageMock.Verify(m => m.RespondAsync("The language changed successfully."), Times.Once);
    }
    
    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal("language", _languageCommand.Name);
        Assert.Equal("Change the bot language.", _languageCommand.Description);
    }
}