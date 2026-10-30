package com.dc.melodiasmario.core.commonui.music.model

import androidx.compose.runtime.Composable

/**
 * Represents add music labels shared between application layers.
 *
 * @property title Title.
 * @property searchMode Search mode.
 * @property manualMode Manual mode.
 * @property modeHelp Mode help.
 * @property providerLabel Provider label.
 * @property searchPlaceholder Search placeholder.
 * @property searchAction Search action.
 * @property tracks Tracks.
 * @property playlists Playlists.
 * @property kindHelp Kind help.
 * @property capabilitiesLoading Capabilities loading.
 * @property providerUnavailable Provider unavailable.
 * @property kindUnavailable Kind unavailable.
 * @property empty Empty.
 * @property error Error.
 * @property retry Retry.
 * @property loadMore Load more.
 * @property addTrack Add track.
 * @property addPlaylist Add playlist.
 * @property manualPlaceholder Manual placeholder.
 * @property manualCancel Manual cancel.
 * @property manualAdd Manual add.
 * @property unknownDuration Unknown duration.
 * @property playlistCount Playlist count.
 * @property unavailableReason Unavailable reason.
 */
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
    val playlistCount: @Composable (trackCount: Int) -> String,
    val unavailableReason: @Composable (reasonKey: String) -> String
)
