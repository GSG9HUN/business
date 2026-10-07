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
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicMode

@Composable
fun MAddMusicModeSelector(
    labels: MAddMusicLabels,
    selectedMode: MAddMusicMode,
    onModeChanged: (MAddMusicMode) -> Unit,
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(8.dp),
    ) {
        MModeButton(
            modifier = Modifier.weight(1f),
            text = labels.searchMode,
            selected = selectedMode == MAddMusicMode.Search,
            onClick = { onModeChanged(MAddMusicMode.Search) },
        )
        MModeButton(
            modifier = Modifier.weight(1f),
            text = labels.manualMode,
            selected = selectedMode == MAddMusicMode.Manual,
            onClick = { onModeChanged(MAddMusicMode.Manual) },
        )
        MHelpTooltipButton(text = labels.modeHelp)
    }
}
