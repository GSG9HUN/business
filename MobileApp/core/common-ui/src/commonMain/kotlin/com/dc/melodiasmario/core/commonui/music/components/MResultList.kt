package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.runtime.Composable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.OutlinedButton
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.commonui.music.state.MAddMusicSheetState

@Composable
fun MResultList(
    state: MAddMusicSheetState,
    labels: MAddMusicLabels,
    onResultClicked: (String) -> Unit,
    onNextPageRequested: () -> Unit,
) {
    LazyColumn(
        modifier = Modifier.fillMaxWidth().heightIn(max = 320.dp),
        verticalArrangement = Arrangement.spacedBy(8.dp),
    ) {
        items(state.results, key = { it.id }) { result ->
            MSearchResultRow(
                result = result,
                labels = labels,
                selected = state.selectedResultId == result.id,
                onClick = { onResultClicked(result.id) },
            )
        }

        if (state.nextPageToken != null) {
            item {
                OutlinedButton(
                    modifier = Modifier.fillMaxWidth(),
                    onClick = onNextPageRequested,
                    enabled = !state.isLoadingNextPage && !state.isSearching,
                ) {
                    MText(text = labels.loadMore)
                }
            }
        }
    }
}