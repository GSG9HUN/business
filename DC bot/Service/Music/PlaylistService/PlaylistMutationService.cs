using DC_bot.Configuration;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence.Exceptions;
using DC_bot.Interface.Service.Persistence.Playlists;
using DC_bot.Validation;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.PlaylistService;

/// <summary>
/// Creates, deletes, and renames guild playlists while enforcing naming and ownership rules.
/// </summary>
/// <param name="playlistRepository">The playlist repository dependency used by this boundary.</param>
/// <param name="options">The options supplied for this operation.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
internal sealed class PlaylistMutationService(
    IPlaylistRepository playlistRepository,
    PlaylistOptions options,
    ILogger<PlaylistService> logger)
{

    /// <summary>
    /// Deletes a guild-owned playlist after validating and normalizing its name.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    internal async Task<DeletePlaylistResult> DeletePlaylistAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!PlaylistNameValidator.TryNormalize(playlistName, out playlistName))
        {
            return DeletePlaylistResult.InvalidPlaylistName;
        }

        try
        {
            var playlist = await playlistRepository.GetByGuildAndNameAsync(guildId, playlistName, cancellationToken: cancellationToken);

            if (playlist is null)
            {
                logger.LogInformation("Playlist {PlaylistName} was not found for guild {GuildId}", playlistName, guildId);
                return DeletePlaylistResult.DoesNotExist;
            }

            var deleted = await playlistRepository.DeleteByGuildAndNameAsync(guildId, playlistName, cancellationToken: cancellationToken);
            return deleted ? DeletePlaylistResult.Deleted : DeletePlaylistResult.UnknownError;
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);
            return DeletePlaylistResult.UnknownError;
        }
    }

    /// <summary>
    /// Renames a guild-owned playlist after validating names and checking for conflicts.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="currentName">The current name supplied for this operation.</param>
    /// <param name="newName">The new name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    internal async Task<RenamePlaylistResult> RenamePlaylistAsync(
        ulong guildId,
        string currentName,
        string newName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!PlaylistNameValidator.TryNormalize(currentName, out currentName)
            || !PlaylistNameValidator.TryNormalize(newName, out newName))
        {
            return RenamePlaylistResult.InvalidPlaylistName;
        }

        try
        {
            var playlist = await playlistRepository.GetByGuildAndNameAsync(guildId, currentName, cancellationToken: cancellationToken);

            if (playlist is null)
            {
                logger.LogInformation("Playlist {PlaylistName} was not found for guild {GuildId}", currentName, guildId);
                return RenamePlaylistResult.PlaylistDoesNotExist;
            }

            var newNameExists = await playlistRepository.ExistsAsync(guildId, newName, cancellationToken: cancellationToken);

            if (newNameExists)
            {
                logger.LogWarning("Playlist {PlaylistName} already exists for guild {GuildId}", newName, guildId);
                return RenamePlaylistResult.PlaylistAlreadyExists;
            }

            var renamed = await playlistRepository.RenameAsync(guildId, currentName, newName, cancellationToken: cancellationToken);

            if (!renamed)
            {
                return RenamePlaylistResult.UnknownError;
            }

            logger.LogInformation("Renamed playlist {CurrentName} to {NewName} for guild {GuildId}", currentName, newName, guildId);
            return RenamePlaylistResult.Renamed;
        }
        catch (UniqueConstraintConflictException ex)
        {
            logger.LogWarning(ex, "Playlist {PlaylistName} already exists for guild {GuildId}", newName, guildId);
            return RenamePlaylistResult.PlaylistAlreadyExists;
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to rename playlist {CurrentName} to {NewName} for guild {GuildId}", currentName, newName, guildId);
            return RenamePlaylistResult.UnknownError;
        }
    }

    /// <summary>
    /// Creates a named empty playlist when its name and the guild&apos;s playlist limit permit it.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    internal async Task<CreatePlaylistResult> CreatePlaylistAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!PlaylistNameValidator.TryNormalize(playlistName, out playlistName))
        {
            return CreatePlaylistResult.InvalidPlaylistName;
        }

        try
        {
            var exists = await playlistRepository.ExistsAsync(guildId, playlistName, cancellationToken: cancellationToken);

            if (exists)
            {
                logger.LogWarning("Playlist {PlaylistName} already exists for guild {GuildId}", playlistName, guildId);
                return CreatePlaylistResult.PlaylistAlreadyExists;
            }

            if (await IsPlaylistLimitReachedAsync(guildId, cancellationToken))
            {
                var maxPlaylists = Math.Max(1, options.MaxPlaylistsPerGuild);
                logger.LogWarning("Playlist limit reached for guild {GuildId}. MaxPlaylistsPerGuild: {MaxPlaylistsPerGuild}", guildId, maxPlaylists);
                return CreatePlaylistResult.PlaylistLimitReached;
            }

            await playlistRepository.CreatePlaylistAsync(guildId, playlistName, cancellationToken: cancellationToken);
            logger.LogInformation("Created new playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);
            return CreatePlaylistResult.Created;
        }
        catch (UniqueConstraintConflictException ex)
        {
            logger.LogWarning(ex, "Playlist {PlaylistName} already exists for guild {GuildId}", playlistName, guildId);
            return CreatePlaylistResult.PlaylistAlreadyExists;
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);
            return CreatePlaylistResult.UnknownError;
        }
    }

    /// <summary>
    /// Checks the guild&apos;s playlist count against the configured limit.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    private async Task<bool> IsPlaylistLimitReachedAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var maxPlaylists = Math.Max(1, options.MaxPlaylistsPerGuild);
        var playlists = await playlistRepository.GetByGuildAsync(guildId, cancellationToken: cancellationToken);
        return playlists.Count >= maxPlaylists;
    }
}
