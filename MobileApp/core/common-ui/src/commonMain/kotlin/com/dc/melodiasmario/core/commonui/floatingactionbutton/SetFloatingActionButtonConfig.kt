package com.dc.melodiasmario.core.commonui.floatingactionbutton

import androidx.compose.runtime.Composable
import androidx.compose.runtime.SideEffect
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.remember

@Composable
fun SetFloatingActionButtonConfig(config: FloatingActionButtonConfig?) {
    val controller = LocalFloatingActionButtonController.current
    val owner = remember { floatingActionButtonOwner() }

    SideEffect {
        controller.set(
            owner = owner,
            config = config,
        )
    }

    DisposableEffect(controller, owner) {
        onDispose {
            controller.clear(owner)
        }
    }
}
