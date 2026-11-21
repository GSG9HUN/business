namespace DC_bot.Interface.Service.Presentation.Models;

/// <summary>Retains typed help navigation independently of the formatted page text.</summary>
/// <param name="View">The displayed view.</param>
/// <param name="Category">The selected category, if any.</param>
/// <param name="CategoryPage">The one-based category page to display or restore.</param>
/// <param name="CommandName">The selected canonical command name, if any.</param>
public sealed record HelpNavigationState(
    HelpView View,
    HelpCategory? Category = null,
    int CategoryPage = 1,
    string? CommandName = null);
