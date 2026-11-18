using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Discovery;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using DC_bot.Service.Presentation;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Music;

/// <summary>Routes favourite through the shared discovery service and localized result presentation.</summary>
/// <param name="service">Executes the discovery behavior with its ownership and validation rules.</param>
/// <param name="localization">Resolves the command description.</param>
/// <param name="responses">Presents localized success and validation results.</param>
/// <param name="logger">Records command invocation and completion.</param>
/// <param name="pagination">Owns caller-bound navigation for long read-only results.</param>
public sealed class FavouriteCommand(
    IFavoriteService service,
    ILocalizationService localization,
    IResponseBuilder responses,
    ILogger<FavouriteCommand> logger,
    DiscordPaginationService pagination) : ICommand
{

    /// <inheritdoc />
    public string Name => "favourite";

    /// <inheritdoc />
    public string Description => localization.Get(DescriptionKey);

    /// <inheritdoc />
    public string DescriptionKey => LocalizationKeys.FavoriteCommandDescription;

    /// <inheritdoc />
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var parts = message.Content.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
        var result = await service.ExecuteAsync(message, parts.Length == 2 ? parts[1] : string.Empty);

        if (!result.Success)
        {
            await responses.SendValidationErrorAsync(message, result.ErrorCode ?? ValidationErrorKeys.LavalinkError, default);

            return;
        }

        if (result.Pagination is { } pages)
        {
            await pagination.SendAsync(message, pages);
            logger.CommandExecuted(Name);
            return;
        }

        await responses.SendSuccessAsync(message, LocalizationKeys.PlaybackCommandResponse, result.Message);
        logger.CommandExecuted(Name);
    }
}
