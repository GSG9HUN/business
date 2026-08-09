package com.dc.melodiasmario.shared.presentation.queue

import androidx.lifecycle.ViewModel
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class QueueViewModel : ViewModel() {
    fun emptyText(): String = ""
}
