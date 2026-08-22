package com.dc.melodiasmario.core.ui.feedback.state

import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
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


@Composable
fun rememberMToastHostState(): MToastHostState {
    return remember { MToastHostState() }
}