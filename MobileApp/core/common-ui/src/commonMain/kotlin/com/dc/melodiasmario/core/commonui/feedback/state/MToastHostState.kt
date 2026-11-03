package com.dc.melodiasmario.core.commonui.feedback.state

import com.dc.melodiasmario.core.commonui.feedback.model.MToastData
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.asSharedFlow

/**
 * Provides toast host state for the state layer.
 */
class MToastHostState {
    private val _toasts = MutableSharedFlow<MToastData>(
        extraBufferCapacity = 1
    )

    /**
     * Toasts exposed by toast host state.
     */
    val toasts: SharedFlow<MToastData> = _toasts.asSharedFlow()

    /**
     * Queues a toast for display by the shared toast host.
     */
    suspend fun showToast(toast: MToastData): Unit {
        _toasts.emit(value = toast)
    }
}
