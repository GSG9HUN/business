namespace DC_bot.Interface.Service.Presentation.Models;

/// <summary>Contains a help view, its immutable command snapshot and presentation text.</summary>
/// <param name="GuildId">The guild language and ownership scope.</param>
/// <param name="Catalog">The command snapshot retained for this help session.</param>
/// <param name="Navigation">The typed view and return-page state.</param>
/// <param name="Page">The bounded formatted page content.</param>
/// <param name="Footer">The localized view-specific footer.</param>
public sealed record HelpResponse(
    ulong GuildId,
    IReadOnlyList<HelpCommandMetadata> Catalog,
    HelpNavigationState Navigation,
    PaginationPage Page,
    string Footer);
