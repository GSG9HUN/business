package com.dc.melodiasmario.core.commonui.components

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyItemScope
import androidx.compose.foundation.lazy.LazyListScope
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.alpha
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.queue_batch_label
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.queue_state_playing
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.queue_state_played
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.queue_state_skipped
import com.dc.melodiasmario.core.model.currenttrack.Track
import org.jetbrains.compose.resources.stringResource

/**
 * Adds read-only batch history while preserving the indices and keys of editable waiting tracks.
 *
 * [key] and [itemContent] receive zero-based indices into [items], excluding history rows.
 */
fun LazyListScope.queueBatchItems(
    items: List<Track>,
    displayTracks: List<Track>,
    key: (index: Int, track: Track) -> Any,
    itemContent: @Composable LazyItemScope.(index: Int, track: Track) -> Unit
): Unit {
    val batchIds = (displayTracks + items).mapNotNull { track -> track.batchId }
        .distinct()
    val history = displayTracks.filter { track -> track.queueState != "Queued" }
        .groupBy { track -> track.batchId }
    val emittedHistory = mutableSetOf<String?>()
    var previousBatch: String? = null

    fun header(batchId: String?, sectionKey: String) {

        if (batchId == null) {
            return
        }

        item(key = sectionKey) {
            MText(
                text = stringResource(resource = Res.string.queue_batch_label, batchIds.indexOf(element = batchId) + 1),
                modifier = Modifier.padding(vertical = 8.dp)
            )
        }
    }

    fun historyRows(batchId: String?) {
        history[batchId].orEmpty()
            .sortedBy { track -> track.batchPosition }
            .forEach { track ->
                item(key = "history:${track.queueItemId}") { QueueBatchHistoryRow(track = track) }
            }
    }

    items.forEachIndexed { index, track ->

        if (index == 0 || track.batchId != previousBatch) {
            header(batchId = track.batchId, sectionKey = "batch:$index:${track.batchId}")
        }

        if (emittedHistory.add(track.batchId)) {
            historyRows(batchId = track.batchId)
        }

        item(key = key(index, track)) { itemContent(index, track) }
        previousBatch = track.batchId
    }
    history.keys.filterNot { batchId -> batchId in emittedHistory }
        .forEach { batchId ->
            header(batchId = batchId, sectionKey = "history-batch:$batchId")
            historyRows(batchId = batchId)
        }
}

/** Displays a read-only completed occurrence, or highlights the currently playing occurrence. */
@Composable
private fun QueueBatchHistoryRow(track: Track) {
    val playing = track.queueState == "Playing"
    val state = when (track.queueState) {
        "Playing" -> Res.string.queue_state_playing
        "Played" -> Res.string.queue_state_played
        else -> Res.string.queue_state_skipped
    }
    Surface(
        modifier = Modifier.fillMaxWidth()
            .alpha(alpha = if (playing) 1f else 0.45f),
        color = if (playing) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceContainer,
        shape = MaterialTheme.shapes.medium
    ) {
        Column(modifier = Modifier.padding(all = 12.dp)) {
            MText(text = track.title)
            MText(text = "${track.artist} · ${stringResource(resource = state)}")
        }
    }
}
