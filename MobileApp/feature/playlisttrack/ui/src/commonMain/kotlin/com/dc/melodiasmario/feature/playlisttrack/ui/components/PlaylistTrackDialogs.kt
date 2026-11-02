package com.dc.melodiasmario.feature.playlisttrack.ui.components

import androidx.compose.material3.AlertDialog
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.designsystem.components.dialog.MMoreAction
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.Res
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_cancel
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_more
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_more_message
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_move_down
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_move_up
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_refresh
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_remove
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_remove_confirm
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_shuffle
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_track_actions
import com.dc.melodiasmario.feature.playlisttrack.presentation.PlaylistTrackDialog
import com.dc.melodiasmario.feature.playlisttrack.presentation.PlaylistTrackEvent
import com.dc.melodiasmario.feature.playlisttrack.presentation.PlaylistTrackUiState
import org.jetbrains.compose.resources.stringResource

/**
 * Renders playlist track dialogs from the supplied parameters and forwards interaction callbacks.
 */
@Composable
internal fun PlaylistTrackDialogs(
    uiState: PlaylistTrackUiState,
    onEvent: (
        playlistTrackEvent: PlaylistTrackEvent
    ) -> Unit
): Unit {
    val dismiss = { onEvent(PlaylistTrackEvent.DismissDialog) }
    when (val dialog = uiState.dialog) {
        PlaylistTrackDialog.None -> Unit
        PlaylistTrackDialog.AddMusic -> PlaylistTrackAddMusicSheet(mAddMusicSheetState = uiState.addMusic, onEvent = onEvent)
        PlaylistTrackDialog.More -> PlaylistTrackActionsSheet(
            title = stringResource(resource = Res.string.playlisttrack_more),
            message = stringResource(resource = Res.string.playlisttrack_more_message),
            actions = listOf(
                MMoreAction(
                    text = stringResource(resource = Res.string.playlisttrack_shuffle),
                    enabled = uiState.canModify && uiState.canReorder && uiState.tracks.size > 1,
                    onClick = { onEvent(PlaylistTrackEvent.Shuffle) }
                ),
                MMoreAction(
                    text = stringResource(resource = Res.string.playlisttrack_refresh),
                    enabled = !uiState.isCommandInFlight && !uiState.isRefreshing,
                    onClick = { onEvent(PlaylistTrackEvent.Refresh); dismiss() }
                )
            ),
            onDismiss = dismiss,
            dismissText = stringResource(resource = Res.string.playlisttrack_cancel)
        )
        is PlaylistTrackDialog.TrackActions -> {
            val index = uiState.tracks.indexOfFirst { playlistTrack -> playlistTrack.orderNumber == dialog.orderNumber }
            val track = uiState.tracks.getOrNull(index = index) ?: return
            PlaylistTrackActionsSheet(
                title = track.title,
                message = stringResource(resource = Res.string.playlisttrack_track_actions),
                actions = listOf(
                    MMoreAction(
                        text = stringResource(resource = Res.string.playlisttrack_move_up),
                        enabled = uiState.canModify && uiState.canReorder && index > 0,
                        onClick = { onEvent(PlaylistTrackEvent.Move(fromIndex = index, toIndex = index - 1)) }
                    ),
                    MMoreAction(
                        text = stringResource(resource = Res.string.playlisttrack_move_down),
                        enabled = uiState.canModify && uiState.canReorder && index < uiState.tracks.lastIndex,
                        onClick = { onEvent(PlaylistTrackEvent.Move(fromIndex = index, toIndex = index + 1)) }
                    ),
                    MMoreAction(
                        text = stringResource(resource = Res.string.playlisttrack_remove),
                        enabled = uiState.canModify,
                        isDestructive = true,
                        onClick = { onEvent(PlaylistTrackEvent.RemoveClicked(orderNumber = track.orderNumber)) }
                    )
                ),
                onDismiss = dismiss,
                dismissText = stringResource(resource = Res.string.playlisttrack_cancel)
            )
        }
        is PlaylistTrackDialog.ConfirmRemove -> AlertDialog(
            onDismissRequest = { if (!uiState.isCommandInFlight) {
                dismiss()
            } },
            title = { MText(
                text = stringResource(resource = Res.string.playlisttrack_remove),
                color = MaterialTheme.colorScheme.onSurface
            ) },
            text = { MText(text = stringResource(resource = Res.string.playlisttrack_remove_confirm)) },
            confirmButton = {
                TextButton(
                    enabled = uiState.canModify,
                    onClick = { onEvent(PlaylistTrackEvent.RemoveConfirmed(orderNumber = dialog.orderNumber)) }
                ) { MText(text = stringResource(resource = Res.string.playlisttrack_remove), color = MaterialTheme.colorScheme.error) }
            },
            dismissButton = {
                TextButton(enabled = !uiState.isCommandInFlight, onClick = dismiss) {
                    MText(text = stringResource(resource = Res.string.playlisttrack_cancel))
                }
            }
        )
    }
}
