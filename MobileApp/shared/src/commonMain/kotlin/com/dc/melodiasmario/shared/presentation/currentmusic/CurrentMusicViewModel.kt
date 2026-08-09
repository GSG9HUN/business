package com.dc.melodiasmario.shared.presentation.currentmusic

import androidx.lifecycle.ViewModel
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class CurrentMusicViewModel : ViewModel() {
    fun emptyText(): String = ""
}
