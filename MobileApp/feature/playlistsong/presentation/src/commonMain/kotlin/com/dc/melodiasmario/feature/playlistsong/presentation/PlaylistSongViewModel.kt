package com.dc.melodiasmario.feature.playlistsong.presentation

import androidx.lifecycle.ViewModel
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class PlaylistSongViewModel : ViewModel() {
    fun emptyText(): String = ""
}

