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
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Icon
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.components.MTopBar
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MFloatingButton
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MRefreshButton
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MSearchButton
import com.dc.melodiasmario.core.commonui.designsystem.components.dialog.MConfirmDialog
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MAvatar
import com.dc.melodiasmario.core.commonui.designsystem.components.input.MTextInputDialog
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.model.playlist.GuildData
import com.dc.melodiasmario.core.model.playlist.Playlist
import com.dc.melodiasmario.feature.playlist.generated.resources.Res
import com.dc.melodiasmario.feature.playlist.generated.resources.ic_add
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_add_content_description
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_cancel
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_count
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_create_action
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_create_title
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_delete_action
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_delete_message
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_delete_title
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_empty_message
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_loading_message
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_name_placeholder
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_no_search_results_message
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_placeholder_title
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
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

@Composable
fun PlaylistsScreen(
    modifier: Modifier = Modifier,
    uiState: PlaylistsUiState,
    onEvent: (PlaylistsEvent) -> Unit,
) {
    Scaffold(
        modifier = modifier,
        topBar = {
            MTopBar(
                modifier = Modifier.fillMaxWidth(),
                title = stringResource(Res.string.playlists_title),
                subTitle = stringResource(Res.string.playlists_count, uiState.filteredPlaylists.size),
                actions = {
                    MRefreshButton(
                        onClick = { onEvent(PlaylistsEvent.RefreshClicked) },
                    )
                    MSearchButton(
                        onClick = { onEvent(PlaylistsEvent.SearchClicked) },
                        contentDescription = stringResource(Res.string.playlists_search_content_description),
                    )
                },
                navigationIcon = {
                    MAvatar(
                        name = uiState.guildData.name,
                        imageUrl = uiState.guildData.avatarUrl,
                        shape = RoundedCornerShape(12.dp),
                        size = 44.dp,
                        backgroundColor = MelodiasMarioThemeTokens.current.primary,
                        contentColor = MelodiasMarioThemeTokens.current.textPrimary,
                        avatarOnClick = { onEvent(PlaylistsEvent.AvatarClicked) },
                    )
                },
            )
        },
        floatingActionButton = {
            MFloatingButton(
                modifier = Modifier,
                size = 45.dp,
                shape = RoundedCornerShape(22.dp),
                onClick = { onEvent(PlaylistsEvent.AddPlaylistClicked) },
                icon = {
                    Icon(
                        painter = painterResource(Res.drawable.ic_add),
                        contentDescription = stringResource(Res.string.playlists_add_content_description),
                    )
                },
            )
        },
    ) { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding),
            horizontalAlignment = Alignment.Start,
        ) {
            when {
                uiState.isLoading -> PlaylistsPlaceholder(
                    title = stringResource(Res.string.playlists_placeholder_title),
                    contentText = stringResource(Res.string.playlists_loading_message),
                )

                uiState.errorMessage != null -> PlaylistsPlaceholder(
                    title = stringResource(Res.string.playlists_placeholder_title),
                    contentText = uiState.errorMessage,
                )

                uiState.filteredPlaylists.isEmpty() -> PlaylistsPlaceholder(
                    title = stringResource(Res.string.playlists_placeholder_title),
                    contentText = if (uiState.searchQuery.isBlank()) {
                        stringResource(Res.string.playlists_empty_message)
                    } else {
                        stringResource(Res.string.playlists_no_search_results_message)
                    },
                )

                else -> LazyVerticalGrid(
                    modifier = Modifier
                        .fillMaxWidth()
                        .weight(1f),
                    columns = GridCells.Fixed(2),
                    contentPadding = PaddingValues(
                        start = 12.dp,
                        top = 16.dp,
                        end = 12.dp,
                        bottom = 96.dp,
                    ),
                    horizontalArrangement = Arrangement.spacedBy(14.dp),
                    verticalArrangement = Arrangement.spacedBy(14.dp),
                ) {
                    items(uiState.filteredPlaylists) { playlist ->
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
                                        playlistName = playlist.name,
                                    )
                                )
                            },
                            onDeleteClick = {
                                onEvent(
                                    PlaylistsEvent.DeletePlaylistClicked(
                                        playlistId = playlist.id,
                                        playlistName = playlist.name,
                                    )
                                )
                            },
                        )
                    }
                }
            }
        }
    }

    if (uiState.dialog is PlaylistsDialog.Create) {
        MTextInputDialog(
            title = stringResource(Res.string.playlists_create_title),
            value = uiState.playlistNameDraft,
            onValueChange = { onEvent(PlaylistsEvent.PlaylistNameDraftChanged(it)) },
            onDismiss = { onEvent(PlaylistsEvent.DialogDismissed) },
            onConfirm = { onEvent(PlaylistsEvent.CreatePlaylistConfirmed) },
            placeholder = stringResource(Res.string.playlists_name_placeholder),
            confirmText = stringResource(Res.string.playlists_create_action),
            dismissText = stringResource(Res.string.playlists_cancel),
            enabled = !uiState.isLoading,
            canConfirm = uiState.playlistNameDraft.trim().isNotEmpty(),
        )
    }

    if (uiState.dialog is PlaylistsDialog.Search) {
        MTextInputDialog(
            title = stringResource(Res.string.playlists_search_content_description),
            value = uiState.searchDraft,
            onValueChange = { onEvent(PlaylistsEvent.SearchDraftChanged(it)) },
            onDismiss = { onEvent(PlaylistsEvent.DialogDismissed) },
            onConfirm = { onEvent(PlaylistsEvent.SearchConfirmed) },
            placeholder = stringResource(Res.string.playlists_search_placeholder),
            confirmText = stringResource(Res.string.playlists_search_action),
            dismissText = stringResource(Res.string.playlists_cancel),
            enabled = !uiState.isLoading,
        )
    }

    if (uiState.dialog is PlaylistsDialog.Rename) {
        MTextInputDialog(
            title = stringResource(Res.string.playlists_rename_title),
            value = uiState.playlistNameDraft,
            onValueChange = { onEvent(PlaylistsEvent.PlaylistNameDraftChanged(it)) },
            onDismiss = { onEvent(PlaylistsEvent.DialogDismissed) },
            onConfirm = { onEvent(PlaylistsEvent.RenamePlaylistConfirmed) },
            placeholder = stringResource(Res.string.playlists_name_placeholder),
            confirmText = stringResource(Res.string.playlists_rename_action),
            dismissText = stringResource(Res.string.playlists_cancel),
            enabled = !uiState.isLoading,
            canConfirm = uiState.playlistNameDraft.trim().isNotEmpty(),
        )
    }

    (uiState.dialog as? PlaylistsDialog.Delete)?.let { dialog ->
        MConfirmDialog(
            title = stringResource(Res.string.playlists_delete_title),
            message = stringResource(Res.string.playlists_delete_message, dialog.playlistName),
            confirmText = stringResource(Res.string.playlists_delete_action),
            dismissText = stringResource(Res.string.playlists_cancel),
            enabled = !uiState.isLoading,
            isDestructive = true,
            onDismiss = { onEvent(PlaylistsEvent.DialogDismissed) },
            onConfirm = { onEvent(PlaylistsEvent.DeletePlaylistConfirmed) },
        )
    }
}

@Composable
@Preview(showBackground = true)
fun PlaylistsScreenPreview() {
    MelodiasMarioTheme(
        themeMode = MelodiasMarioThemeMode.Dark
    ) {
        PlaylistsScreen(
            uiState = PlaylistsUiState(
                playlists = listOf(
                    Playlist(name = "Playlist 1", songCount = 10, duration = 300),
                    Playlist(name = "Playlist 2", songCount = 8, duration = 240),
                    Playlist(name = "Playlist 3", songCount = 15, duration = 30015),
                    Playlist(name = "Playlist 4", songCount = 25, duration = 240015),
                ),
                filteredPlaylists = listOf(
                    Playlist(name = "Playlist 1", songCount = 10, duration = 300),
                    Playlist(name = "Playlist 2", songCount = 8, duration = 240),
                    Playlist(name = "Playlist 3", songCount = 15, duration = 30015),
                    Playlist(name = "Playlist 4", songCount = 25, duration = 240015),
                ),
                guildData = GuildData(name = "Guild Name", avatarUrl = null),
            ),
            onEvent = {},
        )
    }
}

@Composable
@Preview(showBackground = true)
fun PlaylistsScreenEmptyListPreview() {
    MelodiasMarioTheme(
        themeMode = MelodiasMarioThemeMode.Dark
    ) {
        PlaylistsScreen(
            uiState = PlaylistsUiState(
                guildData = GuildData(name = "Guild Name", avatarUrl = null),
            ),
            onEvent = {},
        )
    }
}

@Composable
@Preview(showBackground = true)
fun PlaylistsScreenErrorPreview() {
    MelodiasMarioTheme(
        themeMode = MelodiasMarioThemeMode.Dark
    ) {
        PlaylistsScreen(
            uiState = PlaylistsUiState(
                errorMessage = "An error occurred",
                guildData = GuildData(name = "Guild Name", avatarUrl = null),
            ),
            onEvent = {},
        )
    }
}

@Composable
@Preview(showBackground = true)
fun PlaylistsScreenIsLoadingPreview() {
    MelodiasMarioTheme(
        themeMode = MelodiasMarioThemeMode.Dark
    ) {
        PlaylistsScreen(
            uiState = PlaylistsUiState(
                isLoading = true,
                guildData = GuildData(name = "Guild Name", avatarUrl = null),
            ),
            onEvent = {},
        )
    }
}
