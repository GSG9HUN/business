namespace DC_bot_tests.UnitTests.Commands.TextCommands.Music;

[Trait("Category", "Unit")]
public class PlayCommandMetadataTests : PlayCommandTestBase
{
    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal(PlayCommandName, PlayCommand.Name);
        Assert.Equal(PlayCommandDescriptionValue, PlayCommand.Description);
    }
}
