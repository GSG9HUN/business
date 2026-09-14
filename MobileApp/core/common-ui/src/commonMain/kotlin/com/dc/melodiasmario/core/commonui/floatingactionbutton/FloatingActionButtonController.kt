package com.dc.melodiasmario.core.commonui.floatingactionbutton

import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue


class FloatingActionButtonController {
    var config by mutableStateOf<FloatingActionButtonConfig?>(null)
        private set

    private var owner: FloatingActionButtonOwner? = null

    internal fun set(
        owner: FloatingActionButtonOwner,
        config: FloatingActionButtonConfig?,
    ) {
        this.owner = owner
        this.config = config
    }

    internal fun clear(owner: FloatingActionButtonOwner) {
        if (this.owner === owner) {
            this.owner = null
            config = null
        }
    }
}

@Composable
fun rememberFloatingActionButtonController(): FloatingActionButtonController {
    return remember { FloatingActionButtonController() }
}
