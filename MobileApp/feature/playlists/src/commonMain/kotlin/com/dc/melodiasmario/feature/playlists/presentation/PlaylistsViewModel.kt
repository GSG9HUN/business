package com.dc.melodiasmario.feature.playlists.presentation

import androidx.lifecycle.ViewModel
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class PlaylistsViewModel : ViewModel() {
    fun emptyText(): String = ""
}
