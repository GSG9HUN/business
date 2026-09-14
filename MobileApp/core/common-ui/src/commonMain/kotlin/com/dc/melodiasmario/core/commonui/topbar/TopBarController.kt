package com.dc.melodiasmario.core.commonui.topbar

import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue

class TopBarController {
    var config by mutableStateOf<TopBarConfig?>(null)
        private set

    private var owner: TopBarOwner? = null

    internal fun set(
        owner: TopBarOwner,
        config: TopBarConfig?,
    ) {
        this.owner = owner
        this.config = config
    }

    internal fun clear(owner: TopBarOwner) {
        if (this.owner === owner) {
            this.owner = null
            config = null
        }
    }
}

@Composable
fun rememberTopBarController(): TopBarController {
    return remember { TopBarController() }
}
