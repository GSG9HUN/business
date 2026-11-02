package com.dc.melodiasmario.core.commonui.topbar

import androidx.compose.runtime.ProvidableCompositionLocal
import androidx.compose.runtime.compositionLocalOf

/**
 * Local top bar controller exposed by this module.
 */
val localTopBarController: ProvidableCompositionLocal<TopBarController> = compositionLocalOf {
    TopBarController()
}
