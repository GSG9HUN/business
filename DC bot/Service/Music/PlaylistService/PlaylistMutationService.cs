using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.PlaylistService;

internal sealed class PlaylistMutationService(
    IPlaylistRepository playlistRepository,
    ILogger<PlaylistService> logger)
{
    internal async Task<DeletePlaylistResult> DeletePlaylistAsync(ulong guildId, string playlistName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistName);

        try
        {
            var playlist = await playlistRepository.GetByGuildAndNameAsync(guildId, playlistName);
            if (playlist is null)
            {
                logger.LogInformation("Playlist {PlaylistName} was not found for guild {GuildId}", playlistName, guildId);
                return DeletePlaylistResult.DoesNotExist;
            }

            var deleted = await playlistRepository.DeleteByGuildAndNameAsync(guildId, playlistName);

            return deleted ? DeletePlaylistResult.Deleted : DeletePlaylistResult.UnknownError;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);
            return DeletePlaylistResult.UnknownError;
        }
    }

    internal async Task<RenamePlaylistResult> RenamePlaylistAsync(ulong guildId, string currentName, string newName)
    {
        currentName = currentName.Trim();
        newName = newName.Trim();

        if (!PlaylistNameValidator.IsValid(currentName) || !PlaylistNameValidator.IsValid(newName))
        {
            return RenamePlaylistResult.InvalidPlaylistName;
        }

        try
        {
            var playlist = await playlistRepository.GetByGuildAndNameAsync(guildId, currentName);
            if (playlist is null)
            {
                logger.LogInformation("Playlist {PlaylistName} was not found for guild {GuildId}", currentName, guildId);
                return RenamePlaylistResult.PlaylistDoesNotExist;
            }

            var newNameExists = await playlistRepository.ExistsAsync(guildId, newName);
            if (newNameExists)
            {
                logger.LogWarning("Playlist {PlaylistName} already exists for guild {GuildId}", newName, guildId);
                return RenamePlaylistResult.PlaylistAlreadyExists;
            }

            var renamed = await playlistRepository.RenameAsync(guildId, currentName, newName);
            if (!renamed)
            {
                return RenamePlaylistResult.UnknownError;
            }

            logger.LogInformation("Renamed playlist {CurrentName} to {NewName} for guild {GuildId}", currentName,
                newName, guildId);
            return RenamePlaylistResult.Renamed;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to rename playlist {CurrentName} to {NewName} for guild {GuildId}", currentName,
                newName, guildId);
            return RenamePlaylistResult.UnknownError;
        }
    }

    internal async Task<CreatePlaylistResult> CreatePlaylistAsync(ulong guildId, string playlistName)
    {
        playlistName = playlistName.Trim();

        if (!PlaylistNameValidator.IsValid(playlistName))
        {
            return CreatePlaylistResult.InvalidPlaylistName;
        }

        try
        {
            var exists = await playlistRepository.ExistsAsync(guildId, playlistName);

            if (exists)
            {
                logger.LogWarning("Playlist {PlaylistName} already exists for guild {GuildId}", playlistName, guildId);
                return CreatePlaylistResult.PlaylistAlreadyExists;
            }

            await playlistRepository.CreatePlaylistAsync(guildId, playlistName);
            logger.LogInformation("Created new playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);
            return CreatePlaylistResult.Created;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);
            return CreatePlaylistResult.UnknownError;
        }
    }
}
