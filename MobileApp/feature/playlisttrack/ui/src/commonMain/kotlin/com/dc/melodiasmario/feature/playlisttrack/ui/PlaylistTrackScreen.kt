package com.dc.melodiasmario.feature.playlisttrack.ui

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
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
import com.dc.melodiasmario.core.commonui.reorder.ReorderDragContainer
import com.dc.melodiasmario.core.commonui.reorder.ReorderableListDefaults
import com.dc.melodiasmario.core.commonui.topbar.SetTopBarConfig
import com.dc.melodiasmario.core.commonui.topbar.TopBarAction
import com.dc.melodiasmario.core.commonui.topbar.TopBarConfig
import com.dc.melodiasmario.core.commonui.topbar.TopBarNavigationIcon
import com.dc.melodiasmario.core.model.playlist.PlaylistDetail
import com.dc.melodiasmario.core.model.playlist.PlaylistTrack
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.Res
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_add
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_back
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_drag_handle_content_description
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_empty
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_empty_message
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_load_failed
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_more
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_move_down
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_move_up
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_remove
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_subtitle
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_title
import com.dc.melodiasmario.feature.playlisttrack.presentation.PlaylistTrackEvent
import com.dc.melodiasmario.feature.playlisttrack.presentation.PlaylistTrackUiState
import com.dc.melodiasmario.feature.playlisttrack.ui.components.PlaylistTrackDialogs
import com.dc.melodiasmario.feature.playlisttrack.ui.components.PlaylistTrackHeader
import org.jetbrains.compose.resources.stringResource

/**
 * Renders playlist track screen from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun PlaylistTrackScreen(
    modifier: Modifier = Modifier,
    uiState: PlaylistTrackUiState,
    onEvent: (playlistTrackEvent: PlaylistTrackEvent) -> Unit
): Unit {
    SetTopBarConfig(
        config = TopBarConfig(
            title = uiState.playlist?.name ?: stringResource(resource = Res.string.playlisttrack_title),
            subTitle = uiState.playlist?.let { playlist ->
                stringResource(resource = Res.string.playlisttrack_subtitle, formatArgs = arrayOf<Any>(playlist.tracks.size))
            },
            navigationIcon = TopBarNavigationIcon.Back(
                onClick = { onEvent(PlaylistTrackEvent.BackClicked) },
                contentDescription = stringResource(resource = Res.string.playlisttrack_back)
            ),
            actions = listOf(
                TopBarAction.More(
                    onClick = { onEvent(PlaylistTrackEvent.MoreClicked) },
                    contentDescription = stringResource(resource = Res.string.playlisttrack_more)
                )
            )
        )
    )

    SetFloatingActionButtonConfig(config = null)

    val errorMessage = stringResource(resource = Res.string.playlisttrack_load_failed)
    val removeContentDescription = stringResource(resource = Res.string.playlisttrack_remove)
    val dragHandleContentDescription = stringResource(resource = Res.string.playlisttrack_drag_handle_content_description)
    val moveUpContentDescription = stringResource(resource = Res.string.playlisttrack_move_up)
    val moveDownContentDescription = stringResource(resource = Res.string.playlisttrack_move_down)
    when {
        uiState.isLoading -> MLoadingScreen(modifier = modifier.fillMaxSize())
        uiState.hasLoadError -> MErrorScreen(
            modifier = modifier.fillMaxSize(),
            errorMessage = errorMessage,
            onClick = { onEvent(PlaylistTrackEvent.Refresh) }
        )
        else -> {
            val playlist = uiState.playlist ?: return
            val listState = rememberLazyListState()
            ReorderDragContainer(
                modifier = modifier.fillMaxSize(),
                listState = listState,
                itemKeys = uiState.tracks.map { track -> "track:${track.orderNumber}" },
                enabled = uiState.canModify && uiState.canReorder,
                horizontalContentPadding = 20.dp,
                onMove = { from, to -> onEvent(PlaylistTrackEvent.Move(fromIndex = from, toIndex = to)) },
                dragOverlay = { index ->
                    uiState.tracks.getOrNull(index = index)
                        ?.let { track ->
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
                                reorderEnabled = uiState.canReorder
                            )
                        }
                }
            ) { draggedIndex ->
                LazyColumn(
                    state = listState,
                    modifier = Modifier.fillMaxSize(),
                    contentPadding = PaddingValues(start = 20.dp, end = 20.dp, bottom = 32.dp),
                    userScrollEnabled = draggedIndex == null,
                    verticalArrangement = Arrangement.spacedBy(space = ReorderableListDefaults.ItemSpacing)
                ) {
                    item(key = "header") {
                        PlaylistTrackHeader(
                            artworkUri = playlist.artworkUri,
                            name = playlist.name,
                            trackCount = playlist.tracks.size,
                            totalMinutes = playlist.tracks.sumOf { track -> track.durationSeconds.coerceAtLeast(minimumValue = 0)
                                .toLong() } / 60,
                            enabled = uiState.canModify && playlist.tracks.isNotEmpty(),
                            isBusy = uiState.isCommandInFlight || uiState.isRefreshing,
                            onLoad = { onEvent(PlaylistTrackEvent.LoadPlaylist) }
                        )
                        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
                    }
                    if (uiState.tracks.isEmpty()) {
                        item(key = "empty") {
                            Column(
                                modifier = Modifier.fillMaxWidth()
                                    .padding(vertical = 32.dp),
                                verticalArrangement = Arrangement.spacedBy(space = 8.dp)
                            ) {
                                MText(
                                    text = stringResource(resource = Res.string.playlisttrack_empty),
                                    color = MaterialTheme.colorScheme.onSurface,
                                    fontWeight = FontWeight.Bold
                                )
                                MText(text = stringResource(resource = Res.string.playlisttrack_empty_message))
                            }
                        }
                    }

                    itemsIndexed(
                        items = uiState.tracks,
                        key = { _, track -> "track:${track.orderNumber}" }
                    ) { index, track ->
                        MTrackItemRow(
                            modifier = Modifier.animateItem()
                                .alpha(alpha = if (index == draggedIndex) 0f else 1f),
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
                            onRemove = { onEvent(PlaylistTrackEvent.RemoveClicked(orderNumber = track.orderNumber)) },
                            onMoveUp = { onEvent(PlaylistTrackEvent.Move(fromIndex = index, toIndex = index - 1)) },
                            onMoveDown = { onEvent(PlaylistTrackEvent.Move(fromIndex = index, toIndex = index + 1)) }
                        )
                    }
                    item(key = "add") {
                        OutlinedButton(
                            modifier = Modifier.fillMaxWidth()
                                .padding(top = 16.dp),
                            enabled = uiState.canModify,
                            onClick = { onEvent(PlaylistTrackEvent.AddMusicClicked) },
                            contentPadding = PaddingValues(14.dp)
                        ) {
                            MText(
                                text = stringResource(resource = Res.string.playlisttrack_add),
                                color = MaterialTheme.colorScheme.onSurface,
                                fontWeight = FontWeight.Bold
                            )
                        }
                    }
                }
            }
        }
    }
    PlaylistTrackDialogs(uiState = uiState, onEvent = onEvent)
}

private val previewPlaylist = PlaylistDetail(
    id = "1",
    guildId = "guild",
    name = "Late Night",
    tracks = listOf(
        PlaylistTrack(
            orderNumber = 1,
            title = "Midnight City",
            author = "M83",
            durationSeconds = 243,
            trackUri = "https://example.com/1"
        ),
        PlaylistTrack(
            orderNumber = 2,
            title = "Nightcall",
            author = "Kavinsky",
            durationSeconds = 258,
            trackUri = "https://example.com/2"
        ),
        PlaylistTrack(
            orderNumber = 3,
            title = "After Dark",
            author = "Mr.Kitty",
            durationSeconds = 259,
            trackUri = "https://example.com/3"
        )
    )
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
                onEvent = {}
            )
        }
    }
}
