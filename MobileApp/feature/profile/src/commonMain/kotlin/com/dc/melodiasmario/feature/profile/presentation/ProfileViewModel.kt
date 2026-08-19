package com.dc.melodiasmario.feature.profile.presentation

import androidx.lifecycle.ViewModel
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class ProfileViewModel : ViewModel() {
    fun emptyText(): String = ""
}