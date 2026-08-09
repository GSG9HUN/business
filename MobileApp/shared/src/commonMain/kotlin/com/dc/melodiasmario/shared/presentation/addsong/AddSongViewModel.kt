package com.dc.melodiasmario.shared.presentation.addsong

import androidx.lifecycle.ViewModel
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class AddSongViewModel : ViewModel() {
    fun emptyText(): String = ""
}
