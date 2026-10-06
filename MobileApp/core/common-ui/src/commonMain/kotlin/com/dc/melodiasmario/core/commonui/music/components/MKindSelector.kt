package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.runtime.Composable
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MHelpTooltipButton
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchKind

@Composable
fun MKindSelector(
    labels: MAddMusicLabels,
    selectedKind: MMusicSearchKind,
    onKindChanged: (MMusicSearchKind) -> Unit,
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(8.dp),
    ) {
        MSelectablePill(
            modifier = Modifier.weight(1f),
            text = labels.tracks,
            selected = selectedKind == MMusicSearchKind.Track,
            onClick = { onKindChanged(MMusicSearchKind.Track) },
        )
        MSelectablePill(
            modifier = Modifier.weight(1f),
            text = labels.playlists,
            selected = selectedKind == MMusicSearchKind.Playlist,
            onClick = { onKindChanged(MMusicSearchKind.Playlist) },
        )
        MHelpTooltipButton(text = labels.kindHelp)
    }
}
