package com.dc.melodiasmario.core.ui.feedback.state

import com.dc.melodiasmario.core.ui.feedback.model.MToastData
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.asSharedFlow

class MToastHostState {
    private val _toasts = MutableSharedFlow<MToastData>(
        extraBufferCapacity = 1,
    )

    val toasts = _toasts.asSharedFlow()

    suspend fun showToast(toast: MToastData) {
        _toasts.emit(toast)
    }
}