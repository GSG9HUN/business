using DC_bot.Commands.SlashCommands.Utility;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.SlashCommands.Utility;

[Trait("Category", "Unit")]
public class LanguageSlashCommandTests : SlashCommandTestBase
{
    [Theory]
    [InlineData(SlashLanguage.Eng, "eng")]
    [InlineData(SlashLanguage.Hu, "hu")]
    public async Task Language_ShouldCreateInteractionContextAndDelegateLanguageCodeToExecutor(
        SlashLanguage language,
        string languageCode)
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new LanguageSlashCommand(executor.Object, contextFactory.Object);

        await command.Language(dsharpContext, language);

        contextFactory.Verify(x => x.Create(dsharpContext), Times.Once);
        VerifyRequest(
            executor,
            "language",
            slashContext.Object,
            languageCode,
            requireGuild: true,
            defer: true);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnsupportedLanguage_ShouldThrowWithoutExecuting()
    {
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var command = new LanguageSlashCommand(executor.Object, Mock.Of<ISlashInteractionContextFactory>());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            command.ExecuteAsync(slashContext.Object, (SlashLanguage)999));

        executor.Verify(x => x.ExecuteAsync(It.IsAny<SlashCommandExecutionRequest>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSaveLanguageAndRespondAfterDeferring()
    {
        var context = CreateContext();

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "language",
            context,
            "hu",
            requireGuild: true,
            defer: true);

        Assert.True(context.IsDeferred);
        Assert.Contains("The language changed successfully.", context.TextResponses);
        LocalizationServiceMock.Verify(service => service.SaveLanguage(123UL, "hu"), Times.Once);
    }
}
