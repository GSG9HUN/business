using System.Runtime.CompilerServices;
using DC_bot.Wrapper;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot_tests.UnitTests.Wrapper;

[Trait("Category", "Unit")]
public class SlashInteractionContextFactoryTests
{
    [Fact]
    public void Create_WhenContextIsNull_ThrowsArgumentNullException()
    {
        var factory = new SlashInteractionContextFactory();

        Assert.Throws<ArgumentNullException>(() => factory.Create(null!));
    }

    [Fact]
    public void Create_WhenContextIsProvided_ReturnsWrapper()
    {
        var dsharpContext = (CommandContext)RuntimeHelpers.GetUninitializedObject(typeof(SlashCommandContext));
        var factory = new SlashInteractionContextFactory();

        var context = factory.Create(dsharpContext);

        Assert.IsType<SlashInteractionContextWrapper>(context);
    }
}
