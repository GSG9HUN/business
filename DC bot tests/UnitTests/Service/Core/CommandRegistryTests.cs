using DC_bot.Interface;
using DC_bot.Service.Core;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Core;

[Trait("Category", "Unit")]
public class CommandRegistryTests
{
    [Fact]
    public void TryGetCommand_WhenCommandExists_ReturnsRegisteredCommand()
    {
        var command = CreateCommand("help");
        var registry = new CommandRegistry(() => [command.Object]);

        var found = registry.TryGetCommand("help", out var result);

        Assert.True(found);
        Assert.Same(command.Object, result);
    }

    [Fact]
    public void Commands_MaterializesFactoryOnlyOnce()
    {
        var factoryCalls = 0;
        var command = CreateCommand("help");
        var registry = new CommandRegistry(() =>
        {
            factoryCalls++;
            return [command.Object];
        });

        _ = registry.Commands;
        _ = registry.TryGetCommand("help", out _);
        _ = registry.Commands;

        Assert.Equal(1, factoryCalls);
    }

    [Fact]
    public void TryGetCommand_WhenDuplicateNamesRegistered_ThrowsArgumentException()
    {
        var first = CreateCommand("help");
        var second = CreateCommand("help");
        var registry = new CommandRegistry(() => [first.Object, second.Object]);

        Assert.Throws<ArgumentException>(() => registry.TryGetCommand("help", out _));
    }

    private static Mock<ICommand> CreateCommand(string name)
    {
        var command = new Mock<ICommand>();
        command.SetupGet(item => item.Name).Returns(name);
        command.SetupGet(item => item.Description).Returns($"{name} description");
        return command;
    }
}
