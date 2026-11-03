package com.dc.melodiasmario.core.commonui.floatingactionbutton

import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue

/**
 * Owns floating action button coordination and its lifecycle-bound operations.
 */
class FloatingActionButtonController {
    /**
     * Config exposed by floating action button controller.
     */
    var config: FloatingActionButtonConfig? by mutableStateOf<FloatingActionButtonConfig?>(value = null)
        private set

    private var owner: FloatingActionButtonOwner? = null

    /**
     * Updates the value held by floating action button controller.
     */
    internal fun set(
        owner: FloatingActionButtonOwner,
        config: FloatingActionButtonConfig?
    ): Unit {
        this.owner = owner
        this.config = config
    }

    /**
     * Clears the state and resources owned by FloatingActionButtonController.
     */
    internal fun clear(owner: FloatingActionButtonOwner): Unit {
        if (this.owner === owner) {
            this.owner = null
            config = null
        }
    }
}

/**
 * Remembers floating action button controller for the current composition.
 */
@Composable
fun rememberFloatingActionButtonController(): FloatingActionButtonController {
    return remember { FloatingActionButtonController() }
}
