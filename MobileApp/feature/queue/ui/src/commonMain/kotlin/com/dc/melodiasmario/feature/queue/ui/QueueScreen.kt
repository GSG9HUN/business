package com.dc.melodiasmario.feature.queue.ui

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.foundation.lazy.rememberLazyListState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.alpha
import androidx.compose.ui.graphics.graphicsLayer
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.components.MErrorScreen
import com.dc.melodiasmario.core.commonui.components.MLoadingScreen
import com.dc.melodiasmario.core.commonui.components.MQueueTrackCard
import com.dc.melodiasmario.core.commonui.designsystem.components.dialog.MMoreAction
import com.dc.melodiasmario.core.commonui.designsystem.components.dialog.MMoreActionsDialog
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.components.input.MTextInputDialog
import com.dc.melodiasmario.feature.queue.generated.resources.queue_shuffle_action
import com.dc.melodiasmario.feature.queue.generated.resources.queue_playlist_name
import com.dc.melodiasmario.feature.queue.generated.resources.queue_save
import com.dc.melodiasmario.feature.queue.generated.resources.queue_cancel
import com.dc.melodiasmario.feature.queue.generated.resources.queue_processing
import com.dc.melodiasmario.feature.queue.generated.resources.queue_processing_message
import com.dc.melodiasmario.feature.queue.generated.resources.queue_refresh_action
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res as CommonUiRes
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.queue_action_remove_content_description
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.core.commonui.floatingactionbutton.FloatingActionButtonConfig
import com.dc.melodiasmario.core.commonui.floatingactionbutton.FloatingActionButtonIcon
import com.dc.melodiasmario.core.commonui.floatingactionbutton.SetFloatingActionButtonConfig
import com.dc.melodiasmario.core.commonui.music.components.MAddMusicSheet
import com.dc.melodiasmario.core.commonui.music.components.rememberMAddMusicLabels
import com.dc.melodiasmario.core.commonui.reorder.ReorderDragContainer
import com.dc.melodiasmario.core.commonui.reorder.ReorderableListDefaults
import com.dc.melodiasmario.core.commonui.topbar.SetTopBarConfig
import com.dc.melodiasmario.core.commonui.topbar.TopBarAction
import com.dc.melodiasmario.core.commonui.topbar.TopBarConfig
import com.dc.melodiasmario.core.commonui.topbar.TopBarNavigationIcon
import com.dc.melodiasmario.core.model.currenttrack.Track
import com.dc.melodiasmario.feature.queue.generated.resources.Res
import com.dc.melodiasmario.feature.queue.generated.resources.queue_add_content_description
import com.dc.melodiasmario.feature.queue.generated.resources.queue_clear_action
import com.dc.melodiasmario.feature.queue.generated.resources.queue_drag_handle_content_description
import com.dc.melodiasmario.feature.queue.generated.resources.queue_edit_mode_message
import com.dc.melodiasmario.feature.queue.generated.resources.queue_edit_mode_label
import com.dc.melodiasmario.feature.queue.generated.resources.queue_empty_message
import com.dc.melodiasmario.feature.queue.generated.resources.queue_empty_title
import com.dc.melodiasmario.feature.queue.generated.resources.queue_make_playlist_action
import com.dc.melodiasmario.feature.queue.generated.resources.queue_more_message
import com.dc.melodiasmario.feature.queue.generated.resources.queue_more_title
import com.dc.melodiasmario.feature.queue.generated.resources.queue_more_content_description
import com.dc.melodiasmario.feature.queue.generated.resources.queue_move_down_action
import com.dc.melodiasmario.feature.queue.generated.resources.queue_move_up_action
import com.dc.melodiasmario.feature.queue.generated.resources.queue_summary
import com.dc.melodiasmario.feature.queue.generated.resources.queue_title
import com.dc.melodiasmario.feature.queue.presentation.QueueDialog
import com.dc.melodiasmario.feature.queue.presentation.QueueEvent
import com.dc.melodiasmario.feature.queue.presentation.QueueUiState
import com.dc.melodiasmario.feature.queue.ui.components.QueueEditModeCard
import com.dc.melodiasmario.feature.queue.ui.components.QueueEmptyContent
import org.jetbrains.compose.resources.stringResource

@Composable
fun QueueScreen(
    modifier: Modifier = Modifier,
    uiState: QueueUiState,
    onEvent: (QueueEvent) -> Unit,
) {
    val title = stringResource(Res.string.queue_title)

    val queueReady = !uiState.isLoading && uiState.errorMessage == null
    val canModifyQueue = queueReady && !uiState.isCommandInFlight

    val totalMinutes = uiState.tracks.sumOf {
        it.durationSeconds.coerceAtLeast(0).toLong()
    } / 60L

    val subtitle = if (queueReady) {
        stringResource(
            Res.string.queue_summary,
            uiState.tracks.size,
            totalMinutes,
        )
    } else {
        null
    }

    val queueItemSpacing = ReorderableListDefaults.ItemSpacing
    val removeContentDescription =
        stringResource(CommonUiRes.string.queue_action_remove_content_description)
    val dragHandleContentDescription =
        stringResource(Res.string.queue_drag_handle_content_description)
    val moveUpContentDescription = stringResource(Res.string.queue_move_up_action)
    val moveDownContentDescription = stringResource(Res.string.queue_move_down_action)

    val listState = rememberLazyListState()

    SetTopBarConfig(
        TopBarConfig(
            title = title,
            subTitle = subtitle,
            navigationIcon = TopBarNavigationIcon.Avatar(
                name = uiState.guildName,
                onClick = { onEvent(QueueEvent.GuildClicked) },
                imageUrl = uiState.guildIconUrl,
            ),
            actions = listOf(
                TopBarAction.More(
                    onClick = { onEvent(QueueEvent.MoreClicked) },
                    contentDescription = stringResource(Res.string.queue_more_content_description),
                ),
            ),
        )
    )

    SetFloatingActionButtonConfig(
        if (!uiState.isLoading && uiState.errorMessage == null) {
            FloatingActionButtonConfig(
                icon = FloatingActionButtonIcon.Add,
                contentDescription = stringResource(Res.string.queue_add_content_description),
                onClick = { onEvent(QueueEvent.AddMusicClicked) },
                enabled = !uiState.isCommandInFlight,
            )
        } else {
            null
        }
    )

    Column(
        modifier = modifier.fillMaxSize(),
    ) {
        HorizontalDivider()




        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(
                    horizontal = 12.dp,
                    vertical = 8.dp,
                ),
            horizontalArrangement = Arrangement.spacedBy(8.dp),
        ) {
            OutlinedButton(
                modifier = Modifier.weight(1f),
                onClick = { onEvent(QueueEvent.ShuffleQueueClicked) },
                enabled = canModifyQueue && uiState.tracks.size > 1,
                shape = RoundedCornerShape(12.dp),
                border = BorderStroke(
                    width = 1.dp,
                    color = MaterialTheme.colorScheme.outline,
                ),
                colors = ButtonDefaults.outlinedButtonColors(
                    containerColor = MaterialTheme.colorScheme.surfaceContainerHigh,
                    contentColor = MaterialTheme.colorScheme.onSurface,
                    disabledContainerColor = MaterialTheme.colorScheme.surfaceContainerHigh,
                    disabledContentColor = MaterialTheme.colorScheme.onSurfaceVariant,
                ),
            ) {
                MText(
                    text = stringResource(Res.string.queue_shuffle_action),
                    color = if (
                        canModifyQueue && uiState.tracks.size > 1
                    ) {
                        MaterialTheme.colorScheme.onSurface
                    } else {
                        MaterialTheme.colorScheme.onSurfaceVariant
                    },
                    fontWeight = FontWeight.Bold,
                )
            }

            OutlinedButton(
                modifier = Modifier.weight(1f),
                onClick = { onEvent(QueueEvent.ClearQueueClicked) },
                enabled = canModifyQueue && uiState.tracks.isNotEmpty(),
                shape = RoundedCornerShape(12.dp),
                border = BorderStroke(
                    width = 1.dp,
                    color = MaterialTheme.colorScheme.outline,
                ),
                colors = ButtonDefaults.outlinedButtonColors(
                    containerColor = MaterialTheme.colorScheme.surfaceContainerHigh,
                    contentColor = MaterialTheme.colorScheme.error,
                    disabledContainerColor = MaterialTheme.colorScheme.surfaceContainerHigh,
                    disabledContentColor = MaterialTheme.colorScheme.onSurfaceVariant,
                ),
            ) {
                MText(
                    text = stringResource(Res.string.queue_clear_action),
                    color = if (
                        canModifyQueue && uiState.tracks.isNotEmpty()
                    ) {
                        MaterialTheme.colorScheme.error
                    } else {
                        MaterialTheme.colorScheme.onSurfaceVariant
                    },
                    fontWeight = FontWeight.Bold,
                )
            }
        }



        when {
            uiState.isLoading -> MLoadingScreen(
                modifier = Modifier.fillMaxWidth().weight(1f),
            )

            uiState.errorMessage != null -> MErrorScreen(
                modifier = Modifier.fillMaxWidth().weight(1f),
                errorMessage = requireNotNull(uiState.errorMessage),
                onClick = { onEvent(QueueEvent.RefreshClicked) },
            )

            uiState.tracks.isEmpty() -> QueueEmptyContent(
                modifier = Modifier.fillMaxWidth().weight(1f),
                title = stringResource(Res.string.queue_empty_title),
                message = stringResource(Res.string.queue_empty_message),
            )

            else -> ReorderDragContainer(
                modifier = Modifier
                    .fillMaxWidth()
                    .weight(1f),
                listState = listState,
                itemKeys = uiState.tracks.mapIndexed { index, track ->
                    "${track.id}:$index"
                },
                enabled = !uiState.isCommandInFlight,
                onMove = { fromIndex, toIndex ->
                    val track = uiState.tracks[fromIndex]

                    onEvent(
                        QueueEvent.MoveTrackClicked(
                            trackId = track.id,
                            fromIndex = fromIndex,
                            toIndex = toIndex,
                        )
                    )
                },
                dragOverlay = { index ->
                    val track = uiState.tracks[index]

                    MQueueTrackCard(
                        modifier = Modifier
                            .alpha(0.85f)
                            .graphicsLayer {
                                shadowElevation = 16.dp.toPx()
                            },
                        queueNumber = index + 1,
                        title = track.title,
                        artist = track.artist,
                        durationSeconds = track.durationSeconds,
                        thumbnailUrl = track.thumbnailUrl,
                        removeContentDescription = removeContentDescription,
                        dragHandleContentDescription = dragHandleContentDescription,
                        moveUpContentDescription = moveUpContentDescription,
                        moveDownContentDescription = moveDownContentDescription,
                        isFirst = index == 0,
                        isLast = index == uiState.tracks.lastIndex,
                        enabled = false,
                        dragEnabled = false,
                    )
                },
            ) { draggedIndex ->
                LazyColumn(
                    state = listState,
                    modifier = Modifier.fillMaxSize(),
                    userScrollEnabled = draggedIndex == null,
                    contentPadding = PaddingValues(
                        start = 12.dp,
                        top = 12.dp,
                        end = 12.dp,
                        bottom = 96.dp,
                    ),
                    verticalArrangement = Arrangement.spacedBy(queueItemSpacing),
                ) {
                    itemsIndexed(
                        items = uiState.tracks,
                        key = { index, track -> "${track.id}:$index" },
                    ) { index, track ->
                        MQueueTrackCard(
                            modifier = Modifier
                                .animateItem()
                                .alpha(
                                    if (index == draggedIndex) 0f else 1f
                                ),
                            queueNumber = index + 1,
                            title = track.title,
                            artist = track.artist,
                            durationSeconds = track.durationSeconds,
                            thumbnailUrl = track.thumbnailUrl,
                            removeContentDescription = removeContentDescription,
                            dragHandleContentDescription = dragHandleContentDescription,
                            moveUpContentDescription = moveUpContentDescription,
                            moveDownContentDescription = moveDownContentDescription,
                            isFirst = index == 0,
                            isLast = index == uiState.tracks.lastIndex,
                            enabled = !uiState.isCommandInFlight &&
                                    draggedIndex == null,
                            dragEnabled = false,
                            onRemove = {
                                onEvent(
                                    QueueEvent.RemoveTrackClicked(
                                        trackId = track.id,
                                        index = index,
                                    )
                                )
                            },
                            onMoveUp = {
                                onEvent(
                                    QueueEvent.MoveTrackClicked(
                                        trackId = track.id,
                                        fromIndex = index,
                                        toIndex = index - 1,
                                    )
                                )
                            },
                            onMoveDown = {
                                onEvent(
                                    QueueEvent.MoveTrackClicked(
                                        trackId = track.id,
                                        fromIndex = index,
                                        toIndex = index + 1,
                                    )
                                )
                            },
                        )
                    }
                }
            }
        }

        if (!uiState.isLoading && uiState.errorMessage == null) {
            QueueEditModeCard(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 12.dp, vertical = 14.dp),
                title = stringResource(if (uiState.isCommandInFlight) Res.string.queue_processing else Res.string.queue_edit_mode_label),
                message = stringResource(if (uiState.isCommandInFlight) Res.string.queue_processing_message else Res.string.queue_edit_mode_message),
            )
        }
    }

    if (uiState.dialog is QueueDialog.AddMusic) {
        MAddMusicSheet(
            state = uiState.addMusic,
            labels = rememberMAddMusicLabels(),
            onDismiss = { onEvent(QueueEvent.DialogDismissed) },
            onModeChanged = { onEvent(QueueEvent.AddMusicModeChanged(it)) },
            onManualDraftChanged = { onEvent(QueueEvent.ManualDraftChanged(it)) },
            onManualConfirm = { onEvent(QueueEvent.ManualAddClicked) },
            onSearchQueryChanged = { onEvent(QueueEvent.SearchQueryChanged(it)) },
            onSearchSubmitted = { onEvent(QueueEvent.SearchSubmitted) },
            onProviderChanged = { onEvent(QueueEvent.SearchProviderChanged(it)) },
            onKindChanged = { onEvent(QueueEvent.SearchKindChanged(it)) },
            onResultClicked = { onEvent(QueueEvent.SearchResultClicked(it)) },
            onSelectedResultAddClicked = { onEvent(QueueEvent.SelectedSearchResultAddClicked) },
            onNextPageRequested = { onEvent(QueueEvent.SearchNextPageRequested) },
            onSearchRetryClicked = { onEvent(QueueEvent.SearchRetryClicked) },
            onCapabilitiesRetryClicked = { onEvent(QueueEvent.SearchCapabilitiesRetryClicked) },
        )
    }

    if (uiState.dialog is QueueDialog.MoreActions) {
        MMoreActionsDialog(
            title = stringResource(Res.string.queue_more_title),
            message = stringResource(Res.string.queue_more_message),
            actions = listOf(
                MMoreAction(
                    text = stringResource(Res.string.queue_refresh_action),
                    onClick = { onEvent(QueueEvent.RefreshClicked) },
                    enabled = !uiState.isCommandInFlight && !uiState.isLoading,
                ),
                MMoreAction(
                    text = stringResource(Res.string.queue_make_playlist_action),
                    onClick = {
                        onEvent(QueueEvent.MakePlaylistFromQueueClicked)
                    },
                    enabled = canModifyQueue && uiState.tracks.isNotEmpty(),
                ),
            ),
            onDismiss = { onEvent(QueueEvent.DialogDismissed) },
        )
    }

    if (uiState.dialog is QueueDialog.CreatePlaylist) {
        MTextInputDialog(
            title = stringResource(Res.string.queue_make_playlist_action),
            value = uiState.playlistNameDraft,
            onValueChange = { onEvent(QueueEvent.PlaylistNameChanged(it)) },
            onDismiss = { onEvent(QueueEvent.DialogDismissed) },
            onConfirm = { onEvent(QueueEvent.CreatePlaylistConfirmed) },
            placeholder = stringResource(Res.string.queue_playlist_name),
            confirmText = stringResource(Res.string.queue_save),
            dismissText = stringResource(Res.string.queue_cancel),
            enabled = !uiState.isCommandInFlight,
            canConfirm = uiState.playlistNameDraft.isNotBlank() && uiState.tracks.isNotEmpty(),
        )
    }
}

@Preview
@Composable
fun QueueScreenPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        QueueScreen(
            uiState = QueueUiState(
                isLoading = false,
                guildName = "Business business",
                tracks = previewTracks,
            ),
            onEvent = {},
        )
    }
}

private val previewTracks = listOf(
    Track(
        id = "1",
        title = "Midnight City",
        artist = "M83",
        durationSeconds = 243,
    ),
    Track(
        id = "2",
        title = "Blinding Lights",
        artist = "The Weeknd",
        durationSeconds = 200,
    ),
    Track(
        id = "3",
        title = "Sweet Disposition",
        artist = "The Temper Trap",
        durationSeconds = 232,
    ),
)
