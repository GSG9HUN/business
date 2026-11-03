package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.material3.OutlinedButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState

/**
 * Renders result list from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MResultList(
    mAddMusicSheetState: MAddMusicSheetState,
    mAddMusicLabels: MAddMusicLabels,
    onResultClicked: (resultId: String) -> Unit,
    onNextPageRequested: () -> Unit
): Unit {
    LazyColumn(
        modifier = Modifier.fillMaxWidth()
            .heightIn(max = 320.dp),
        verticalArrangement = Arrangement.spacedBy(space = 8.dp)
    ) {
        itemsIndexed(
            items = mAddMusicSheetState.results,
            key = { index, result -> "${result.id}:$index" }
        ) { _, result ->
            MSearchResultRow(
                result = result,
                mAddMusicLabels = mAddMusicLabels,
                selected = mAddMusicSheetState.selectedResultId == result.id,
                onClick = { onResultClicked(result.id) }
            )
        }

        if (mAddMusicSheetState.nextPageToken != null) {
            item {
                OutlinedButton(
                    modifier = Modifier.fillMaxWidth(),
                    onClick = onNextPageRequested,
                    enabled = !mAddMusicSheetState.isLoadingNextPage && !mAddMusicSheetState.isSearching
                ) {
                    MText(text = mAddMusicLabels.loadMore)
                }
            }
        }
    }
}
