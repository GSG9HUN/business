using DC_bot.Commands.SlashCommands.Music;
using DC_bot.Commands.SlashCommands.Utility;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using System.ComponentModel;

namespace DC_bot_tests.UnitTests.Commands.SlashCommands;

[Trait("Category", "Unit")]
public class SlashCommandMetadataTests
{
    [Theory]
    [InlineData(
        typeof(PlaySlashCommand),
        nameof(PlaySlashCommand.Play),
        "play",
        "Start playing music in the voice channel",
        "query",
        typeof(string),
        "URL or search query")]
    [InlineData(
        typeof(TagSlashCommand),
        nameof(TagSlashCommand.Tag),
        "tag",
        "You can tag someone",
        "user",
        typeof(DiscordMember),
        "Member you want to tag.")]
    public void SlashCommandMethod_ShouldExposeDSharpPlusRegistrationMetadata(
        Type commandType,
        string methodName,
        string expectedCommandName,
        string expectedCommandDescription,
        string expectedOptionName,
        Type expectedOptionType,
        string expectedOptionDescription)
    {
        var method = commandType.GetMethod(methodName);

        Assert.NotNull(method);
        var command = Assert.Single(
            method.GetCustomAttributes(typeof(CommandAttribute), inherit: false).Cast<CommandAttribute>());
        var description = Assert.Single(
            method.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false).Cast<DescriptionAttribute>());
        var option = method.GetParameters()[1];
        var optionMetadata = Assert.Single(
            option.GetCustomAttributes(typeof(ParameterAttribute), inherit: false).Cast<ParameterAttribute>());
        var optionDescription = Assert.Single(
            option.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false).Cast<DescriptionAttribute>());

        Assert.Equal(expectedCommandName, command.Name);
        Assert.Equal(expectedCommandDescription, description.Description);
        Assert.Equal(expectedOptionName, optionMetadata.Name);
        Assert.Equal(expectedOptionType, option.ParameterType);
        Assert.Equal(expectedOptionDescription, optionDescription.Description);
    }
}