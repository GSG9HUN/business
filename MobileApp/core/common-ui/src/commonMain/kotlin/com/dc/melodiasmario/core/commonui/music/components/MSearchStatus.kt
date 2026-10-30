package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchProviderUi
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState

/**
 * Renders search status from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MSearchStatus(
    mAddMusicSheetState: MAddMusicSheetState,
    mAddMusicLabels: MAddMusicLabels,
    providerUnavailable: Boolean,
    kindUnavailable: Boolean,
    selectedProvider: MMusicSearchProviderUi?,
    onCapabilitiesRetryClicked: () -> Unit,
    onSearchRetryClicked: () -> Unit
): Unit {
    val message = when {
        mAddMusicSheetState.isCapabilitiesLoading -> mAddMusicLabels.capabilitiesLoading
        providerUnavailable -> {
            val reason = selectedProvider?.unavailableReason

            if (reason != null) {
                mAddMusicLabels.unavailableReason(reason)
            } else {
                mAddMusicLabels.providerUnavailable
            }
        }

        kindUnavailable -> mAddMusicLabels.kindUnavailable
        mAddMusicSheetState.errorMessage != null -> mAddMusicSheetState.errorMessage
        mAddMusicSheetState.isSearching -> mAddMusicLabels.searchAction
        mAddMusicSheetState.results.isEmpty() -> mAddMusicLabels.empty
        else -> null
    }

    message?.let { statusMessage ->
        Row(
            modifier = Modifier.fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(space = 8.dp)
        ) {
            MText(
                modifier = Modifier.weight(weight = 1f),
                text = statusMessage,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                textAlign = TextAlign.Start
            )

            if (mAddMusicSheetState.errorMessage != null) {
                OutlinedButton(onClick = onSearchRetryClicked) {
                    MText(text = mAddMusicLabels.retry)
                }
            } else if (providerUnavailable) {
                OutlinedButton(onClick = onCapabilitiesRetryClicked) {
                    MText(text = mAddMusicLabels.retry)
                }
            }
        }
    }
}
