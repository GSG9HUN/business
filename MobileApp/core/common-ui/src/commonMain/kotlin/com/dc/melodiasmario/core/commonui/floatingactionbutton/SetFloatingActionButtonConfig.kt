package com.dc.melodiasmario.core.commonui.floatingactionbutton

import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.SideEffect
import androidx.compose.runtime.remember

/**
 * Renders set floating action button config from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun SetFloatingActionButtonConfig(config: FloatingActionButtonConfig?): Unit {
    val controller = localFloatingActionButtonController.current
    val owner = remember { floatingActionButtonOwner() }

    SideEffect {
        controller.set(
            owner = owner,
            config = config
        )
    }

    DisposableEffect(key1 = controller, key2 = owner) {
        onDispose {
            controller.clear(owner = owner)
        }
    }
}
