package com.dc.melodiasmario.feature.settings.presentation

import androidx.lifecycle.ViewModel
import org.koin.core.annotation.KoinViewModel

/**
 * Coordinates settings events, observable UI state, and one-time effects.
 */
@KoinViewModel
class SettingsViewModel : ViewModel() {
    /**
     * Provides the empty text operation for settings view model.
     */
    fun emptyText(): String = ""
}
