package com.dc.melodiasmario.core.commonui.biometric

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.presentation.biometric.AppLockController

/**
 * Renders bind app lock lifecycle from the supplied parameters and forwards interaction callbacks.
 */
@Composable
expect fun BindAppLockLifecycle(
    appLockController: AppLockController
): Unit
