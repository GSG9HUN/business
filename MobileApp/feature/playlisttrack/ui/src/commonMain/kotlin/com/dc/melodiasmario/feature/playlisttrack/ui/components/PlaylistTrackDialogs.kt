package com.dc.melodiasmario.feature.playlisttrack.ui.components

import androidx.compose.material3.AlertDialog
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.designsystem.components.dialog.MMoreAction
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.*
import com.dc.melodiasmario.feature.playlisttrack.presentation.*
import org.jetbrains.compose.resources.stringResource

@Composable
internal fun PlaylistTrackDialogs(state: PlaylistTrackUiState, onEvent: (PlaylistTrackEvent) -> Unit) {
    val dismiss = { onEvent(PlaylistTrackEvent.DismissDialog) }
    when (val dialog = state.dialog) {
        PlaylistTrackDialog.None -> Unit
        PlaylistTrackDialog.AddMusic -> PlaylistTrackAddMusicSheet(state.addMusic, onEvent)
        PlaylistTrackDialog.More -> PlaylistTrackActionsSheet(
            title = stringResource(Res.string.playlisttrack_more),
            message = stringResource(Res.string.playlisttrack_more_message),
            actions = listOf(
                MMoreAction(
                    text = stringResource(Res.string.playlisttrack_shuffle),
                    enabled = state.canModify && state.canReorder && state.tracks.size > 1,
                    onClick = { onEvent(PlaylistTrackEvent.Shuffle) },
                ),
                MMoreAction(
                    text = stringResource(Res.string.playlisttrack_refresh),
                    enabled = !state.isCommandInFlight && !state.isRefreshing,
                    onClick = { onEvent(PlaylistTrackEvent.Refresh); dismiss() },
                ),
            ),
            onDismiss = dismiss,
            dismissText = stringResource(Res.string.playlisttrack_cancel),
        )
        is PlaylistTrackDialog.TrackActions -> {
            val index = state.tracks.indexOfFirst { it.orderNumber == dialog.orderNumber }
            val track = state.tracks.getOrNull(index) ?: return
            PlaylistTrackActionsSheet(
                title = track.title,
                message = stringResource(Res.string.playlisttrack_track_actions),
                actions = listOf(
                    MMoreAction(
                        text = stringResource(Res.string.playlisttrack_move_up),
                        enabled = state.canModify && state.canReorder && index > 0,
                        onClick = { onEvent(PlaylistTrackEvent.Move(index, index - 1)) },
                    ),
                    MMoreAction(
                        text = stringResource(Res.string.playlisttrack_move_down),
                        enabled = state.canModify && state.canReorder && index < state.tracks.lastIndex,
                        onClick = { onEvent(PlaylistTrackEvent.Move(index, index + 1)) },
                    ),
                    MMoreAction(
                        text = stringResource(Res.string.playlisttrack_remove),
                        enabled = state.canModify,
                        isDestructive = true,
                        onClick = { onEvent(PlaylistTrackEvent.RemoveClicked(track.orderNumber)) },
                    ),
                ),
                onDismiss = dismiss,
                dismissText = stringResource(Res.string.playlisttrack_cancel),
            )
        }
        is PlaylistTrackDialog.ConfirmRemove -> AlertDialog(
            onDismissRequest = { if (!state.isCommandInFlight) dismiss() },
            title = { MText(
                text = stringResource(Res.string.playlisttrack_remove),
                color = MaterialTheme.colorScheme.onSurface,
            ) },
            text = { MText(text = stringResource(Res.string.playlisttrack_remove_confirm)) },
            confirmButton = {
                TextButton(
                    enabled = state.canModify,
                    onClick = { onEvent(PlaylistTrackEvent.RemoveConfirmed(dialog.orderNumber)) },
                ) { MText(text = stringResource(Res.string.playlisttrack_remove), color = MaterialTheme.colorScheme.error) }
            },
            dismissButton = {
                TextButton(enabled = !state.isCommandInFlight, onClick = dismiss) {
                    MText(text = stringResource(Res.string.playlisttrack_cancel))
                }
            },
        )
    }
}
