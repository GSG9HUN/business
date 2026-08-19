package com.dc.melodiasmario.core.ui.feedback.state

import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember

@Composable
fun rememberMToastHostState(): MToastHostState {
    return remember { MToastHostState() }
}