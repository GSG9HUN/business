namespace DC_bot.Interface.Service.Presentation.Models;

/// <summary>Describes registered command guidance without depending on Discord SDK builders.</summary>
/// <param name="Name">The canonical registered command name.</param>
/// <param name="DescriptionKey">The command's authoritative description key.</param>
/// <param name="Category">Its help category.</param>
/// <param name="RequirementsKey">The localized prerequisite key.</param>
/// <param name="ExampleKey">The localized command example template key.</param>
public sealed record HelpCommandMetadata(
    string Name,
    string DescriptionKey,
    HelpCategory Category,
    string RequirementsKey,
    string ExampleKey);
