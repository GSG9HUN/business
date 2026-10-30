package com.dc.melodiasmario.core.commonui.topbar

import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue

/**
 * Owns top bar coordination and its lifecycle-bound operations.
 */
class TopBarController {
    /**
     * Config exposed by top bar controller.
     */
    var config: TopBarConfig? by mutableStateOf<TopBarConfig?>(value = null)
        private set

    private var owner: TopBarOwner? = null

    /**
     * Updates the value held by top bar controller.
     */
    internal fun set(
        owner: TopBarOwner,
        config: TopBarConfig?
    ): Unit {
        this.owner = owner
        this.config = config
    }

    /**
     * Clears the state and resources owned by TopBarController.
     */
    internal fun clear(owner: TopBarOwner): Unit {
        if (this.owner === owner) {
            this.owner = null
            config = null
        }
    }
}

/**
 * Remembers top bar controller for the current composition.
 */
@Composable
fun rememberTopBarController(): TopBarController {
    return remember { TopBarController() }
}
