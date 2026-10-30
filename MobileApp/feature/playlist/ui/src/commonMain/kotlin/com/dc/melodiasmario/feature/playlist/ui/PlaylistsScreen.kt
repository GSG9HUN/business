package com.dc.melodiasmario.feature.playlist.ui

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.components.MErrorScreen
import com.dc.melodiasmario.core.commonui.components.MLoadingScreen
import com.dc.melodiasmario.core.commonui.designsystem.components.dialog.MConfirmDialog
import com.dc.melodiasmario.core.commonui.designsystem.components.input.MTextInputDialog
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.core.commonui.floatingactionbutton.FloatingActionButtonConfig
import com.dc.melodiasmario.core.commonui.floatingactionbutton.FloatingActionButtonIcon
import com.dc.melodiasmario.core.commonui.floatingactionbutton.SetFloatingActionButtonConfig
import com.dc.melodiasmario.core.commonui.topbar.SetTopBarConfig
import com.dc.melodiasmario.core.commonui.topbar.TopBarAction
import com.dc.melodiasmario.core.commonui.topbar.TopBarConfig
import com.dc.melodiasmario.core.commonui.topbar.TopBarNavigationIcon
import com.dc.melodiasmario.core.model.playlist.GuildData
import com.dc.melodiasmario.core.model.playlist.Playlist
import com.dc.melodiasmario.feature.playlist.generated.resources.Res
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_add_content_description
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_artwork_placeholder
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_artwork_save
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_artwork_title
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_cancel
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_count
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_create_action
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_create_title
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_delete_action
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_delete_message
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_delete_title
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_empty_message
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_name_placeholder
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_no_search_results_message
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_placeholder_title
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_refresh_content_description
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_rename_action
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_rename_title
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_search_action
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_search_content_description
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_search_placeholder
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_title
import com.dc.melodiasmario.feature.playlist.presentation.PlaylistsDialog
import com.dc.melodiasmario.feature.playlist.presentation.PlaylistsEvent
import com.dc.melodiasmario.feature.playlist.presentation.PlaylistsUiState
import com.dc.melodiasmario.feature.playlist.ui.components.PlaylistCard
import com.dc.melodiasmario.feature.playlist.ui.components.PlaylistsPlaceholder
import org.jetbrains.compose.resources.stringResource

/**
 * Renders playlists screen from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun PlaylistsScreen(
    modifier: Modifier = Modifier,
    uiState: PlaylistsUiState,
    onEvent: (playlistsEvent: PlaylistsEvent) -> Unit
): Unit {
    SetTopBarConfig(
        config = TopBarConfig(
            title = stringResource(resource = Res.string.playlists_title),
            subTitle = stringResource(
                resource = Res.string.playlists_count,
                formatArgs = arrayOf<Any>(uiState.filteredPlaylists.size)
            ),
            navigationIcon = TopBarNavigationIcon.Avatar(
                name = uiState.guildData.name,
                imageUrl = uiState.guildData.avatarUrl,
                onClick = { onEvent(PlaylistsEvent.AvatarClicked) }
            ),
            actions = listOf(
                TopBarAction.Refresh(
                    onClick = { onEvent(PlaylistsEvent.RefreshClicked) },
                    contentDescription = stringResource(resource = Res.string.playlists_refresh_content_description)
                ),
                TopBarAction.Search(
                    onClick = { onEvent(PlaylistsEvent.SearchClicked) },
                    contentDescription = stringResource(
                        resource = Res.string.playlists_search_content_description
                    )
                )
            )
        )
    )

    SetFloatingActionButtonConfig(
        config = if (!uiState.isLoading && uiState.errorMessage == null) {
            FloatingActionButtonConfig(
                icon = FloatingActionButtonIcon.Add,
                contentDescription = stringResource(resource = Res.string.playlists_add_content_description),
                onClick = { onEvent(PlaylistsEvent.AddPlaylistClicked) }
            )
        } else {
            null
        }
    )

    Column(
        modifier = modifier.fillMaxSize(),
        horizontalAlignment = Alignment.Start
    ) {
        when {
            uiState.isLoading -> MLoadingScreen(
                modifier = Modifier.fillMaxWidth()
                    .weight(weight = 1f)
            )

            uiState.errorMessage != null -> MErrorScreen(
                modifier = Modifier.fillMaxWidth()
                    .weight(weight = 1f),
                errorMessage = requireNotNull(value = uiState.errorMessage),
                onClick = { onEvent(PlaylistsEvent.RefreshClicked) }
            )

            uiState.filteredPlaylists.isEmpty() -> PlaylistsPlaceholder(
                modifier = Modifier.padding(horizontal = 0.dp, vertical = 16.dp),
                title = stringResource(resource = Res.string.playlists_placeholder_title),
                contentText = if (uiState.searchQuery.isBlank()) {
                    stringResource(resource = Res.string.playlists_empty_message)
                } else {
                    stringResource(resource = Res.string.playlists_no_search_results_message)
                }
            )

            else -> LazyVerticalGrid(
                modifier = Modifier
                    .fillMaxWidth()
                    .weight(weight = 1f),
                columns = GridCells.Fixed(count = 2),
                contentPadding = PaddingValues(
                    start = 12.dp,
                    top = 16.dp,
                    end = 12.dp,
                    bottom = 96.dp
                ),
                horizontalArrangement = Arrangement.spacedBy(space = 14.dp),
                verticalArrangement = Arrangement.spacedBy(space = 14.dp)
            ) {
                items(items = uiState.filteredPlaylists) { playlist ->
                    PlaylistCard(
                        modifier = Modifier,
                        playlist = playlist,
                        onClick = {
                            onEvent(PlaylistsEvent.PlaylistClicked(playlistId = playlist.id))
                        },
                        onRenameClick = {
                            onEvent(
                                PlaylistsEvent.RenamePlaylistClicked(
                                    playlistId = playlist.id,
                                    playlistName = playlist.name
                                )
                            )
                        },
                        onDeleteClick = {
                            onEvent(
                                PlaylistsEvent.DeletePlaylistClicked(
                                    playlistId = playlist.id,
                                    playlistName = playlist.name
                                )
                            )
                        },
                        onArtworkClick = { onEvent(PlaylistsEvent.ArtworkClicked(playlistId = playlist.id)) }
                    )
                }
            }
        }
    }

    if (uiState.dialog is PlaylistsDialog.Artwork) {
        MTextInputDialog(
            title = stringResource(resource = Res.string.playlists_artwork_title),
            value = uiState.artworkDraft,
            onValueChange = { inputValue -> onEvent(PlaylistsEvent.ArtworkDraftChanged(artworkUri = inputValue)) },
            onDismiss = { onEvent(PlaylistsEvent.DialogDismissed) },
            onConfirm = { onEvent(PlaylistsEvent.ArtworkConfirmed) },
            placeholder = stringResource(resource = Res.string.playlists_artwork_placeholder),
            confirmText = stringResource(resource = Res.string.playlists_artwork_save),
            dismissText = stringResource(resource = Res.string.playlists_cancel),
            enabled = !uiState.isLoading,
            canConfirm = uiState.artworkDraft.length <= 2048
        )
    }

    if (uiState.dialog is PlaylistsDialog.Create) {
        MTextInputDialog(
            title = stringResource(resource = Res.string.playlists_create_title),
            value = uiState.playlistNameDraft,
            onValueChange = { inputValue -> onEvent(PlaylistsEvent.PlaylistNameDraftChanged(playlistName = inputValue)) },
            onDismiss = { onEvent(PlaylistsEvent.DialogDismissed) },
            onConfirm = { onEvent(PlaylistsEvent.CreatePlaylistConfirmed) },
            placeholder = stringResource(resource = Res.string.playlists_name_placeholder),
            confirmText = stringResource(resource = Res.string.playlists_create_action),
            dismissText = stringResource(resource = Res.string.playlists_cancel),
            enabled = !uiState.isLoading,
            canConfirm = uiState.playlistNameDraft.trim()
                .isNotEmpty()
        )
    }

    if (uiState.dialog is PlaylistsDialog.Search) {
        MTextInputDialog(
            title = stringResource(resource = Res.string.playlists_search_content_description),
            value = uiState.searchDraft,
            onValueChange = { inputValue -> onEvent(PlaylistsEvent.SearchDraftChanged(query = inputValue)) },
            onDismiss = { onEvent(PlaylistsEvent.DialogDismissed) },
            onConfirm = { onEvent(PlaylistsEvent.SearchConfirmed) },
            placeholder = stringResource(resource = Res.string.playlists_search_placeholder),
            confirmText = stringResource(resource = Res.string.playlists_search_action),
            dismissText = stringResource(resource = Res.string.playlists_cancel),
            enabled = !uiState.isLoading
        )
    }

    if (uiState.dialog is PlaylistsDialog.Rename) {
        MTextInputDialog(
            title = stringResource(resource = Res.string.playlists_rename_title),
            value = uiState.playlistNameDraft,
            onValueChange = { inputValue -> onEvent(PlaylistsEvent.PlaylistNameDraftChanged(playlistName = inputValue)) },
            onDismiss = { onEvent(PlaylistsEvent.DialogDismissed) },
            onConfirm = { onEvent(PlaylistsEvent.RenamePlaylistConfirmed) },
            placeholder = stringResource(resource = Res.string.playlists_name_placeholder),
            confirmText = stringResource(resource = Res.string.playlists_rename_action),
            dismissText = stringResource(resource = Res.string.playlists_cancel),
            enabled = !uiState.isLoading,
            canConfirm = uiState.playlistNameDraft.trim()
                .isNotEmpty()
        )
    }

    (uiState.dialog as? PlaylistsDialog.Delete)?.let { dialog ->
        MConfirmDialog(
            title = stringResource(resource = Res.string.playlists_delete_title),
            message = stringResource(resource = Res.string.playlists_delete_message, formatArgs = arrayOf<Any>(dialog.playlistName)),
            confirmText = stringResource(resource = Res.string.playlists_delete_action),
            dismissText = stringResource(resource = Res.string.playlists_cancel),
            enabled = !uiState.isLoading,
            isDestructive = true,
            onDismiss = { onEvent(PlaylistsEvent.DialogDismissed) },
            onConfirm = { onEvent(PlaylistsEvent.DeletePlaylistConfirmed) }
        )
    }
}

@Composable
@Preview(showBackground = true)
private fun PlaylistsScreenPreview() {
    MelodiasMarioTheme(
        themeMode = MelodiasMarioThemeMode.Dark
    ) {
        PlaylistsScreen(
            uiState = PlaylistsUiState(
                playlists = listOf(
                    Playlist(name = "Playlist 1", songCount = 10, duration = 300),
                    Playlist(name = "Playlist 2", songCount = 8, duration = 240),
                    Playlist(name = "Playlist 3", songCount = 15, duration = 30015),
                    Playlist(name = "Playlist 4", songCount = 25, duration = 240015)
                ),
                filteredPlaylists = listOf(
                    Playlist(name = "Playlist 1", songCount = 10, duration = 300),
                    Playlist(name = "Playlist 2", songCount = 8, duration = 240),
                    Playlist(name = "Playlist 3", songCount = 15, duration = 30015),
                    Playlist(name = "Playlist 4", songCount = 25, duration = 240015)
                ),
                guildData = GuildData(name = "Guild Name", avatarUrl = null)
            ),
            onEvent = {}
        )
    }
}

@Composable
@Preview(showBackground = true)
private fun PlaylistsScreenEmptyListPreview() {
    MelodiasMarioTheme(
        themeMode = MelodiasMarioThemeMode.Dark
    ) {
        PlaylistsScreen(
            uiState = PlaylistsUiState(
                guildData = GuildData(name = "Guild Name", avatarUrl = null)
            ),
            onEvent = {}
        )
    }
}

@Composable
@Preview(showBackground = true)
private fun PlaylistsScreenErrorPreview() {
    MelodiasMarioTheme(
        themeMode = MelodiasMarioThemeMode.Dark
    ) {
        PlaylistsScreen(
            uiState = PlaylistsUiState(
                errorMessage = "An error occurred",
                guildData = GuildData(name = "Guild Name", avatarUrl = null)
            ),
            onEvent = {}
        )
    }
}

@Composable
@Preview(showBackground = true)
private fun PlaylistsScreenIsLoadingPreview() {
    MelodiasMarioTheme(
        themeMode = MelodiasMarioThemeMode.Dark
    ) {
        PlaylistsScreen(
            uiState = PlaylistsUiState(
                isLoading = true,
                guildData = GuildData(name = "Guild Name", avatarUrl = null)
            ),
            onEvent = {}
        )
    }
}
