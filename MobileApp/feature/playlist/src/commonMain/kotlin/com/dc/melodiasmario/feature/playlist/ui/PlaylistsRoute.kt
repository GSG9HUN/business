package com.dc.melodiasmario.feature.playlist.ui

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import org.koin.compose.viewmodel.koinViewModel
import com.dc.melodiasmario.feature.playlist.presentation.PlaylistsEffect
import com.dc.melodiasmario.feature.playlist.presentation.PlaylistsEvent
import com.dc.melodiasmario.feature.playlist.presentation.PlaylistsViewModel

@Composable
fun PlaylistsRoute(
    guildId: String,
    modifier: Modifier = Modifier,
    viewModel: PlaylistsViewModel = koinViewModel(),
    onPlaylistClicked: (playlistId: String) -> Unit = {},
    onAvatarClicked: () -> Unit = {},
) {

    val uiState by viewModel.uiState.collectAsState()

    LaunchedEffect(guildId) {
        viewModel.onEvent(PlaylistsEvent.LoadPlaylists(guildId))
    }

    LaunchedEffect(viewModel) {
        viewModel.effect.collect { effect ->
            when (effect) {
                PlaylistsEffect.NavigateToGuildSelector -> onAvatarClicked()
                is PlaylistsEffect.NavigateToPlaylistSongs -> onPlaylistClicked(effect.playlistId)
            }
        }
    }

    PlaylistsScreen(
        modifier = modifier,
        uiState = uiState,
        onEvent = viewModel::onEvent
    )
}
