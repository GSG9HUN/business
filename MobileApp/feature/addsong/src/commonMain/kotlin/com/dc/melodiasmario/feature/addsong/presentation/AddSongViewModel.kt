package com.dc.melodiasmario.feature.addsong.presentation

import androidx.lifecycle.ViewModel
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class AddSongViewModel : ViewModel() {
    fun emptyText(): String = ""
}
