package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.material3.OutlinedButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchProviderUi
import com.dc.melodiasmario.core.commonui.music.state.MAddMusicSheetState

@Composable
fun MSearchStatus(
    state: MAddMusicSheetState,
    labels: MAddMusicLabels,
    providerUnavailable: Boolean,
    kindUnavailable: Boolean,
    selectedProvider: MMusicSearchProviderUi?,
    onCapabilitiesRetryClicked: () -> Unit,
    onSearchRetryClicked: () -> Unit,
) {
    val message = when {
        state.isCapabilitiesLoading -> labels.capabilitiesLoading
        providerUnavailable -> {
            val reason = selectedProvider?.unavailableReason
            if (reason != null) labels.unavailableReason(reason) else labels.providerUnavailable
        }

        kindUnavailable -> labels.kindUnavailable
        state.errorMessage != null -> state.errorMessage
        state.isSearching -> labels.searchAction
        state.results.isEmpty() -> labels.empty
        else -> null
    }

    message?.let {
        Row(
            modifier = Modifier.fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(8.dp),
        ) {
            MText(
                modifier = Modifier.weight(1f),
                text = it,
                color = MelodiasMarioThemeTokens.current.textMuted,
                textAlign = TextAlign.Start,
            )
            if (state.errorMessage != null) {
                OutlinedButton(onClick = onSearchRetryClicked) {
                    MText(text = labels.retry)
                }
            } else if (providerUnavailable) {
                OutlinedButton(onClick = onCapabilitiesRetryClicked) {
                    MText(text = labels.retry)
                }
            }
        }
    }
}