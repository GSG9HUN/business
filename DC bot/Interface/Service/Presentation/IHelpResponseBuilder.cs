using DC_bot.Interface.Service.Presentation.Models;
using DSharpPlus.Entities;

namespace DC_bot.Interface.Service.Presentation;

/// <summary>Builds and navigates localized help views; message delivery and lifetime belong to pagination.</summary>
public interface IHelpResponseBuilder
{

    /// <summary>Creates an overview or direct command view; unknown command names return null.</summary>
    HelpResponse? Create(ulong guildId, string? commandName, CancellationToken cancellationToken = default);

    /// <summary>Resolves an available action against the displayed snapshot; invalid actions return null.</summary>
    HelpResponse? Navigate(
        HelpResponse response,
        HelpNavigationAction action,
        string? selection,
        CancellationToken cancellationToken = default);

    /// <summary>Renders a V2 view with versioned controls, omitting controls after expiry.</summary>
    DiscordMessageBuilder BuildMessage(HelpResponse response, string componentPrefix, bool expired);
}
