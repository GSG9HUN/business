namespace DC_bot.Interface.Service.Presentation.Models;

/// <summary>Contains one bounded read-only page and its current navigation metadata.</summary>
/// <param name="Title">The localized panel title.</param>
/// <param name="Description">The formatted page body.</param>
/// <param name="Number">The one-based displayed page.</param>
/// <param name="PageCount">The current number of pages.</param>
/// <param name="TotalItems">The total item count, omitted for continuous text.</param>
public sealed record PaginationPage(string Title, string Description, int Number, int PageCount, int? TotalItems = null);