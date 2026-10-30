package com.dc.melodiasmario.core.commonui.floatingactionbutton

import androidx.compose.runtime.ProvidableCompositionLocal
import androidx.compose.runtime.compositionLocalOf

/**
 * Local floating action button controller exposed by this module.
 */
val localFloatingActionButtonController: ProvidableCompositionLocal<FloatingActionButtonController> = compositionLocalOf {
    FloatingActionButtonController()
}
