package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchResultUi

@Composable
internal fun MMusicSearchResultUi.subtitle(labels: MAddMusicLabels): String {
    val playlistItemCount = itemCount
    val detail = when {
        kind == MMusicSearchKind.Playlist && playlistItemCount != null -> labels.playlistCount(playlistItemCount)
        durationText != null -> durationText
        kind == MMusicSearchKind.Track -> labels.unknownDuration
        else -> null
    }
    return listOfNotNull(creator, providerName, detail).joinToString(" - ")
}
