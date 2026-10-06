package com.dc.melodiasmario.core.commonui.music.model

import androidx.compose.runtime.Composable

data class MAddMusicLabels(
    val title: String,
    val searchMode: String,
    val manualMode: String,
    val modeHelp: String,

    val providerLabel: String,
    val searchPlaceholder: String,
    val searchAction: String,

    val tracks: String,
    val playlists: String,
    val kindHelp: String,

    val capabilitiesLoading: String,
    val providerUnavailable: String,
    val kindUnavailable: String,
    val empty: String,
    val error: String,
    val retry: String,
    val loadMore: String,

    val addTrack: String,
    val addPlaylist: String,

    val manualPlaceholder: String,
    val manualCancel: String,
    val manualAdd: String,

    val unknownDuration: String,
    val playlistCount: @Composable (Int) -> String,
    val unavailableReason: @Composable (String) -> String,
)
