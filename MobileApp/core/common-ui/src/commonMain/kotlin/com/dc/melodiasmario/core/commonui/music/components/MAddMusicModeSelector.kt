package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MHelpTooltipButton
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MModeButton
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.presentation.music.model.MAddMusicMode

/**
 * Renders add music mode selector from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MAddMusicModeSelector(
    mAddMusicLabels: MAddMusicLabels,
    selectedMode: MAddMusicMode,
    onModeChanged: (addMusicMode: MAddMusicMode) -> Unit
): Unit {
    Row(
        modifier = Modifier.fillMaxWidth(),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(space = 8.dp)
    ) {
        MModeButton(
            modifier = Modifier.weight(weight = 1f),
            text = mAddMusicLabels.searchMode,
            selected = selectedMode == MAddMusicMode.Search,
            onClick = { onModeChanged(MAddMusicMode.Search) }
        )
        MModeButton(
            modifier = Modifier.weight(weight = 1f),
            text = mAddMusicLabels.manualMode,
            selected = selectedMode == MAddMusicMode.Manual,
            onClick = { onModeChanged(MAddMusicMode.Manual) }
        )
        MHelpTooltipButton(text = mAddMusicLabels.modeHelp)
    }
}
