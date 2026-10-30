package com.dc.melodiasmario.feature.playlist.ui

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.feedback.model.MToastData
import com.dc.melodiasmario.core.commonui.feedback.model.MToastType
import com.dc.melodiasmario.feature.playlist.generated.resources.Res
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_artwork_failed
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_artwork_success
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_create_failed
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_create_success
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_delete_failed
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_delete_success
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_rename_failed
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_rename_success
import com.dc.melodiasmario.feature.playlist.presentation.PlaylistsEffect
import org.jetbrains.compose.resources.stringResource

/**
 * Provides playlists effect messages for the ui layer.
 */
internal data class PlaylistsEffectMessages(
    private val createSuccess: String,
    private val createFailed: String,
    private val renameSuccess: String,
    private val renameFailed: String,
    private val deleteSuccess: String,
    private val deleteFailed: String,
    private val artworkSuccess: String,
    private val artworkFailed: String
) {
    /**
     * Maps the supplied effect to localized toast data, or returns null when no toast is needed.
     */
    fun toastDataFor(effect: PlaylistsEffect): MToastData? = when (effect) {
        PlaylistsEffect.ArtworkSaved -> success(message = artworkSuccess)
        PlaylistsEffect.ArtworkSaveFailed -> error(message = artworkFailed)
        PlaylistsEffect.NavigateToGuildSelector,
        is PlaylistsEffect.NavigateToPlaylistTracks -> null

        PlaylistsEffect.PlaylistCreated -> success(message = createSuccess)
        PlaylistsEffect.PlaylistCreateFailed -> error(message = createFailed)
        PlaylistsEffect.PlaylistRenamed -> success(message = renameSuccess)
        PlaylistsEffect.PlaylistRenameFailed -> error(message = renameFailed)
        PlaylistsEffect.PlaylistDeleted -> success(message = deleteSuccess)
        PlaylistsEffect.PlaylistDeleteFailed -> error(message = deleteFailed)
    }

    private fun success(message: String) = MToastData(
        message = message,
        type = MToastType.Success
    )

    private fun error(message: String) = MToastData(
        message = message,
        type = MToastType.Error
    )
}

/**
 * Provides playlists effect messages for the current composition.
 */
@Composable
internal fun playlistsEffectMessages(): PlaylistsEffectMessages = PlaylistsEffectMessages(
    artworkSuccess = stringResource(resource = Res.string.playlists_artwork_success),
    artworkFailed = stringResource(resource = Res.string.playlists_artwork_failed),
    createSuccess = stringResource(resource = Res.string.playlists_create_success),
    createFailed = stringResource(resource = Res.string.playlists_create_failed),
    renameSuccess = stringResource(resource = Res.string.playlists_rename_success),
    renameFailed = stringResource(resource = Res.string.playlists_rename_failed),
    deleteSuccess = stringResource(resource = Res.string.playlists_delete_success),
    deleteFailed = stringResource(resource = Res.string.playlists_delete_failed)
)
