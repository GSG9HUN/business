package com.dc.melodiasmario.shared.presentation.profile

import androidx.lifecycle.ViewModel
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class ProfileViewModel : ViewModel() {
    fun emptyText(): String = ""
}
