package com.dc.melodiasmario.shared.presentation.playlists

import androidx.lifecycle.ViewModel
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class PlaylistSongsViewModel : ViewModel() {
    fun emptyText(): String = ""
}
