package com.dc.melodiasmario.feature.playlist.ui

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.commonui.feedback.model.MToastData
import com.dc.melodiasmario.core.commonui.feedback.model.MToastType
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.playlist.generated.resources.Res
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_create_failed
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_create_success
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_delete_failed
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_delete_success
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_rename_failed
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_rename_success
import org.koin.compose.viewmodel.koinViewModel
import com.dc.melodiasmario.feature.playlist.presentation.PlaylistsEffect
import com.dc.melodiasmario.feature.playlist.presentation.PlaylistsEvent
import com.dc.melodiasmario.feature.playlist.presentation.PlaylistsViewModel
import org.jetbrains.compose.resources.stringResource

@Composable
fun PlaylistsRoute(
    guildId: String,
    modifier: Modifier = Modifier,
    viewModel: PlaylistsViewModel = koinViewModel(),
    onPlaylistClicked: (playlistId: String) -> Unit = {},
    onAvatarClicked: () -> Unit = {},
    toastHostState: MToastHostState,
) {

    val uiState by viewModel.uiState.collectAsState()
    val createSuccessText = stringResource(Res.string.playlists_create_success)
    val createFailedText = stringResource(Res.string.playlists_create_failed)
    val renameSuccessText = stringResource(Res.string.playlists_rename_success)
    val renameFailedText = stringResource(Res.string.playlists_rename_failed)
    val deleteSuccessText = stringResource(Res.string.playlists_delete_success)
    val deleteFailedText = stringResource(Res.string.playlists_delete_failed)

    LaunchedEffect(guildId) {
        viewModel.onEvent(PlaylistsEvent.LoadPlaylists(guildId))
    }

    LaunchedEffect(viewModel) {
        viewModel.effect.collect { effect ->
            when (effect) {
                PlaylistsEffect.NavigateToGuildSelector -> onAvatarClicked()
                is PlaylistsEffect.NavigateToPlaylistSongs -> onPlaylistClicked(effect.playlistId)
                PlaylistsEffect.PlaylistCreated -> {
                    toastHostState.showToast(
                        MToastData(
                            message = createSuccessText,
                            type = MToastType.Success,
                        )
                    )
                }
                PlaylistsEffect.PlaylistCreateFailed -> {
                    toastHostState.showToast(
                        MToastData(
                            message = createFailedText,
                            type = MToastType.Error,
                        )
                    )
                }
                PlaylistsEffect.PlaylistRenamed -> {
                    toastHostState.showToast(
                        MToastData(
                            message = renameSuccessText,
                            type = MToastType.Success,
                        )
                    )
                }
                PlaylistsEffect.PlaylistRenameFailed -> {
                    toastHostState.showToast(
                        MToastData(
                            message = renameFailedText,
                            type = MToastType.Error,
                        )
                    )
                }
                PlaylistsEffect.PlaylistDeleted -> {
                    toastHostState.showToast(
                        MToastData(
                            message = deleteSuccessText,
                            type = MToastType.Success,
                        )
                    )
                }
                PlaylistsEffect.PlaylistDeleteFailed -> {
                    toastHostState.showToast(
                        MToastData(
                            message = deleteFailedText,
                            type = MToastType.Error,
                        )
                    )
                }
            }
        }
    }

    PlaylistsScreen(
        modifier = modifier,
        uiState = uiState,
        onEvent = viewModel::onEvent
    )
}
