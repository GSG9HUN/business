package com.dc.melodiasmario.feature.removesong.presentation

import androidx.lifecycle.ViewModel
import org.koin.core.annotation.KoinViewModel

/**
 * Coordinates remove song events, observable UI state, and one-time effects.
 */
@KoinViewModel
class RemoveSongViewModel : ViewModel() {
    /**
     * Provides the empty text operation for remove song view model.
     */
    fun emptyText(): String = ""
}
