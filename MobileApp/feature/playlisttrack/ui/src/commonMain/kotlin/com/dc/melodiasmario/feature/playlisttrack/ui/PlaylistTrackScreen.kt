package com.dc.melodiasmario.feature.playlisttrack.ui

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.foundation.lazy.rememberLazyListState
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.alpha
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.components.MErrorScreen
import com.dc.melodiasmario.core.commonui.components.MLoadingScreen
import com.dc.melodiasmario.core.commonui.components.MTrackItemRow
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.core.commonui.floatingactionbutton.SetFloatingActionButtonConfig
import com.dc.melodiasmario.core.commonui.topbar.SetTopBarConfig
import com.dc.melodiasmario.core.commonui.topbar.TopBarAction
import com.dc.melodiasmario.core.commonui.topbar.TopBarConfig
import com.dc.melodiasmario.core.commonui.topbar.TopBarNavigationIcon
import com.dc.melodiasmario.core.commonui.reorder.ReorderDragContainer
import com.dc.melodiasmario.core.commonui.reorder.ReorderableListDefaults
import com.dc.melodiasmario.core.model.playlist.PlaylistDetail
import com.dc.melodiasmario.core.model.playlist.PlaylistTrack
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.*
import com.dc.melodiasmario.feature.playlisttrack.presentation.*
import com.dc.melodiasmario.feature.playlisttrack.ui.components.*
import org.jetbrains.compose.resources.stringResource

@Composable
fun PlaylistTrackScreen(
    modifier: Modifier = Modifier,
    uiState: PlaylistTrackUiState,
    onEvent: (PlaylistTrackEvent) -> Unit,
) {
    SetTopBarConfig(
        TopBarConfig(
            title = uiState.playlist?.name ?: stringResource(Res.string.playlisttrack_title),
            subTitle = uiState.playlist?.let {
                stringResource(Res.string.playlisttrack_subtitle, it.tracks.size)
            },
            navigationIcon = TopBarNavigationIcon.Back(
                onClick = { onEvent(PlaylistTrackEvent.BackClicked) },
                contentDescription = stringResource(Res.string.playlisttrack_back),
            ),
            actions = listOf(
                TopBarAction.More(
                    onClick = { onEvent(PlaylistTrackEvent.MoreClicked) },
                    contentDescription = stringResource(Res.string.playlisttrack_more),
                ),
            ),
        )
    )

    SetFloatingActionButtonConfig(null)

    val errorMessage = stringResource(Res.string.playlisttrack_load_failed)
    val removeContentDescription = stringResource(Res.string.playlisttrack_remove)
    val dragHandleContentDescription = stringResource(Res.string.playlisttrack_drag_handle_content_description)
    val moveUpContentDescription = stringResource(Res.string.playlisttrack_move_up)
    val moveDownContentDescription = stringResource(Res.string.playlisttrack_move_down)
    when {
        uiState.isLoading -> MLoadingScreen(modifier = modifier.fillMaxSize())
        uiState.hasLoadError -> MErrorScreen(
            modifier = modifier.fillMaxSize(),
            errorMessage = errorMessage,
            onClick = { onEvent(PlaylistTrackEvent.Refresh) },
        )
        else -> {
            val playlist = uiState.playlist ?: return
            val listState = rememberLazyListState()
            ReorderDragContainer(
                modifier = modifier.fillMaxSize(),
                listState = listState,
                itemKeys = uiState.tracks.map { "track:${it.orderNumber}" },
                enabled = uiState.canModify && uiState.canReorder,
                horizontalContentPadding = 20.dp,
                onMove = { from, to -> onEvent(PlaylistTrackEvent.Move(from, to)) },
                dragOverlay = { index ->
                    uiState.tracks.getOrNull(index)?.let { track ->
                        MTrackItemRow(
                            trackNumber = index + 1,
                            title = track.title,
                            artist = track.author,
                            thumbnailUrl = track.artworkUri,
                            durationSeconds = track.durationSeconds,
                            removeContentDescription = removeContentDescription,
                            dragHandleContentDescription = dragHandleContentDescription,
                            moveUpContentDescription = moveUpContentDescription,
                            moveDownContentDescription = moveDownContentDescription,
                            isFirst = index == 0,
                            isLast = index == uiState.tracks.lastIndex,
                            enabled = false,
                            dragEnabled = false,
                            reorderEnabled = uiState.canReorder,
                        )
                    }
                },
            ) { draggedIndex ->
                LazyColumn(
                    state = listState,
                    modifier = Modifier.fillMaxSize(),
                    contentPadding = PaddingValues(start = 20.dp, end = 20.dp, bottom = 32.dp),
                    userScrollEnabled = draggedIndex == null,
                    verticalArrangement = Arrangement.spacedBy(ReorderableListDefaults.ItemSpacing),
                ) {
                    item(key = "header") {
                        PlaylistTrackHeader(
                            artworkUri = playlist.artworkUri,
                            name = playlist.name,
                            trackCount = playlist.tracks.size,
                            totalMinutes = playlist.tracks.sumOf { it.durationSeconds.coerceAtLeast(0).toLong() } / 60,
                            enabled = uiState.canModify && playlist.tracks.isNotEmpty(),
                            isBusy = uiState.isCommandInFlight || uiState.isRefreshing,
                            onLoad = { onEvent(PlaylistTrackEvent.LoadPlaylist) },
                        )
                        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
                    }
                    if (uiState.tracks.isEmpty()) {
                        item(key = "empty") {
                            Column(
                                modifier = Modifier.fillMaxWidth().padding(vertical = 32.dp),
                                verticalArrangement = Arrangement.spacedBy(8.dp),
                            ) {
                                MText(
                                    text = stringResource(Res.string.playlisttrack_empty),
                                    color = MaterialTheme.colorScheme.onSurface,
                                    fontWeight = FontWeight.Bold,
                                )
                                MText(text = stringResource(Res.string.playlisttrack_empty_message))
                            }
                        }
                    }
                    itemsIndexed(
                        items = uiState.tracks,
                        key = { _, track -> "track:${track.orderNumber}" },
                    ) { index, track ->
                        MTrackItemRow(
                            modifier = Modifier.animateItem().alpha(if (index == draggedIndex) 0f else 1f),
                            trackNumber = index + 1,
                            title = track.title,
                            artist = track.author,
                            durationSeconds = track.durationSeconds,
                            thumbnailUrl = track.artworkUri,
                            removeContentDescription = removeContentDescription,
                            dragHandleContentDescription = dragHandleContentDescription,
                            moveUpContentDescription = moveUpContentDescription,
                            moveDownContentDescription = moveDownContentDescription,
                            isFirst = index == 0,
                            isLast = index == uiState.tracks.lastIndex,
                            enabled = uiState.canModify && draggedIndex == null,
                            dragEnabled = false,
                            reorderEnabled = uiState.canReorder,
                            onRemove = { onEvent(PlaylistTrackEvent.RemoveClicked(track.orderNumber)) },
                            onMoveUp = { onEvent(PlaylistTrackEvent.Move(index, index - 1)) },
                            onMoveDown = { onEvent(PlaylistTrackEvent.Move(index, index + 1)) },
                        )
                    }
                    item(key = "add") {
                        OutlinedButton(
                            modifier = Modifier.fillMaxWidth().padding(top = 16.dp),
                            enabled = uiState.canModify,
                            onClick = { onEvent(PlaylistTrackEvent.AddMusicClicked) },
                            contentPadding = PaddingValues(14.dp),
                        ) {
                            MText(
                                text = stringResource(Res.string.playlisttrack_add),
                                color = MaterialTheme.colorScheme.onSurface,
                                fontWeight = FontWeight.Bold,
                            )
                        }
                    }
                }
            }
        }
    }
    PlaylistTrackDialogs(uiState, onEvent)
}

private val previewPlaylist = PlaylistDetail(
    id = "1",
    guildId = "guild",
    name = "Late Night",
    tracks = listOf(
        PlaylistTrack(1, "Midnight City", "M83", 243, "https://example.com/1"),
        PlaylistTrack(2, "Nightcall", "Kavinsky", 258, "https://example.com/2"),
        PlaylistTrack(3, "After Dark", "Mr.Kitty", 259, "https://example.com/3"),
    ),
)

@Preview
@Composable
private fun PlaylistTrackDarkPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        Surface {
            PlaylistTrackScreen(uiState = PlaylistTrackUiState(isLoading = false, playlist = previewPlaylist), onEvent = {})
        }
    }
}

@Preview
@Composable
private fun PlaylistTrackLightPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Light) {
        Surface {
            PlaylistTrackScreen(uiState = PlaylistTrackUiState(isLoading = false, playlist = previewPlaylist), onEvent = {})
        }
    }
}

@Preview
@Composable
private fun PlaylistTrackEmptyPreview() {
    MelodiasMarioTheme {
        Surface {
            PlaylistTrackScreen(
                uiState = PlaylistTrackUiState(isLoading = false, playlist = previewPlaylist.copy(tracks = emptyList())),
                onEvent = {},
            )
        }
    }
}
