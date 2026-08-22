package com.dc.melodiasmario.feature.queue.presentation

import androidx.lifecycle.ViewModel
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class QueueViewModel : ViewModel() {
    fun emptyText(): String = ""
}
