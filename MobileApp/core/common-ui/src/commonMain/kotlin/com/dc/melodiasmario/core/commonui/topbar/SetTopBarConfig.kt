package com.dc.melodiasmario.core.commonui.topbar

import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.SideEffect
import androidx.compose.runtime.remember

@Composable
fun SetTopBarConfig(config: TopBarConfig?) {
    val topBarController = LocalTopBarController.current
    val owner = remember { topBarOwner() }

    SideEffect {
        topBarController.set(
            owner = owner,
            config = config,
        )
    }

    DisposableEffect(topBarController, owner) {
        onDispose {
            topBarController.clear(owner)
        }
    }
}
