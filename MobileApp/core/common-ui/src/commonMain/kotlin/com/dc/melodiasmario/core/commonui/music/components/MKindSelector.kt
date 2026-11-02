package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MHelpTooltipButton
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchKind

/**
 * Renders kind selector from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MKindSelector(
    mAddMusicLabels: MAddMusicLabels,
    selectedKind: MMusicSearchKind,
    onKindChanged: (musicSearchKind: MMusicSearchKind) -> Unit
): Unit {
    Row(
        modifier = Modifier.fillMaxWidth(),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(space = 8.dp)
    ) {
        MSelectablePill(
            modifier = Modifier.weight(weight = 1f),
            text = mAddMusicLabels.tracks,
            selected = selectedKind == MMusicSearchKind.Track,
            onClick = { onKindChanged(MMusicSearchKind.Track) }
        )
        MSelectablePill(
            modifier = Modifier.weight(weight = 1f),
            text = mAddMusicLabels.playlists,
            selected = selectedKind == MMusicSearchKind.Playlist,
            onClick = { onKindChanged(MMusicSearchKind.Playlist) }
        )
        MHelpTooltipButton(text = mAddMusicLabels.kindHelp)
    }
}
