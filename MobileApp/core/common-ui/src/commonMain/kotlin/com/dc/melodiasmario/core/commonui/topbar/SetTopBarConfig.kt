package com.dc.melodiasmario.core.commonui.topbar

import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.SideEffect
import androidx.compose.runtime.remember

/**
 * Renders set top bar config from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun SetTopBarConfig(config: TopBarConfig?): Unit {
    val topBarController = localTopBarController.current
    val owner = remember { topBarOwner() }

    SideEffect {
        topBarController.set(
            owner = owner,
            config = config
        )
    }

    DisposableEffect(key1 = topBarController, key2 = owner) {
        onDispose {
            topBarController.clear(owner = owner)
        }
    }
}
