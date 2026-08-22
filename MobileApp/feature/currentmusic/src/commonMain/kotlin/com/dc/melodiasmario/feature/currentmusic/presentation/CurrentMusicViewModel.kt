package com.dc.melodiasmario.feature.currentmusic.presentation

import androidx.lifecycle.ViewModel
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class CurrentMusicViewModel : ViewModel() {
    fun emptyText(): String = ""
}
