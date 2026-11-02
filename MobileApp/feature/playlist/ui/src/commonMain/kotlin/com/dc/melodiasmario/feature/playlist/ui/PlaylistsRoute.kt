package com.dc.melodiasmario.feature.playlist.ui

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.playlist.presentation.PlaylistsEffect
import com.dc.melodiasmario.feature.playlist.presentation.PlaylistsEvent
import com.dc.melodiasmario.feature.playlist.presentation.PlaylistsViewModel
import org.koin.compose.viewmodel.koinViewModel

/**
 * Connects playlists presentation state and effects to its screen callbacks.
 */
@Composable
fun PlaylistsRoute(
    guildId: String,
    modifier: Modifier = Modifier,
    playlistsViewModel: PlaylistsViewModel = koinViewModel(),
    onPlaylistClicked: (playlistId: String) -> Unit = {},
    onAvatarClicked: () -> Unit = {},
    toastHostState: MToastHostState
): Unit {

    val uiState by playlistsViewModel.uiState.collectAsState()
    val effectMessages = playlistsEffectMessages()

    LaunchedEffect(key1 = guildId) {
        playlistsViewModel.onEvent(event = PlaylistsEvent.LoadPlaylists(guildId = guildId))
    }

    LaunchedEffect(key1 = playlistsViewModel) {
        playlistsViewModel.effect.collect { effect ->
            when (effect) {
                PlaylistsEffect.NavigateToGuildSelector -> onAvatarClicked()
                is PlaylistsEffect.NavigateToPlaylistTracks -> onPlaylistClicked(effect.playlistId)
                else -> effectMessages.toastDataFor(effect = effect)
                    ?.let { toastData -> toastHostState.showToast(toast = toastData) }
            }
        }
    }

    PlaylistsScreen(
        modifier = modifier,
        uiState = uiState,
        onEvent = playlistsViewModel::onEvent
    )
}
