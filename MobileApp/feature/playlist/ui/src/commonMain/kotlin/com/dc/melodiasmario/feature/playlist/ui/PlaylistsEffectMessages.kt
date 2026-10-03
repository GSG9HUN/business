package com.dc.melodiasmario.feature.playlist.ui

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.feedback.model.MToastData
import com.dc.melodiasmario.core.commonui.feedback.model.MToastType
import com.dc.melodiasmario.feature.playlist.generated.resources.Res
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_create_failed
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_create_success
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_delete_failed
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_delete_success
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_rename_failed
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_rename_success
import com.dc.melodiasmario.feature.playlist.presentation.PlaylistsEffect
import org.jetbrains.compose.resources.stringResource

internal data class PlaylistsEffectMessages(
    private val createSuccess: String,
    private val createFailed: String,
    private val renameSuccess: String,
    private val renameFailed: String,
    private val deleteSuccess: String,
    private val deleteFailed: String,
) {
    fun toastDataFor(effect: PlaylistsEffect): MToastData? = when (effect) {
        PlaylistsEffect.NavigateToGuildSelector,
        is PlaylistsEffect.NavigateToPlaylistSongs -> null

        PlaylistsEffect.PlaylistCreated -> success(createSuccess)
        PlaylistsEffect.PlaylistCreateFailed -> error(createFailed)
        PlaylistsEffect.PlaylistRenamed -> success(renameSuccess)
        PlaylistsEffect.PlaylistRenameFailed -> error(renameFailed)
        PlaylistsEffect.PlaylistDeleted -> success(deleteSuccess)
        PlaylistsEffect.PlaylistDeleteFailed -> error(deleteFailed)
    }

    private fun success(message: String) = MToastData(
        message = message,
        type = MToastType.Success,
    )

    private fun error(message: String) = MToastData(
        message = message,
        type = MToastType.Error,
    )
}

@Composable
internal fun playlistsEffectMessages() = PlaylistsEffectMessages(
    createSuccess = stringResource(Res.string.playlists_create_success),
    createFailed = stringResource(Res.string.playlists_create_failed),
    renameSuccess = stringResource(Res.string.playlists_rename_success),
    renameFailed = stringResource(Res.string.playlists_rename_failed),
    deleteSuccess = stringResource(Res.string.playlists_delete_success),
    deleteFailed = stringResource(Res.string.playlists_delete_failed),
)
