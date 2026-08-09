package com.dc.melodiasmario.shared.presentation.settings

import androidx.lifecycle.ViewModel
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class SettingsViewModel : ViewModel() {
    fun emptyText(): String = ""
}
