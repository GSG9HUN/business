package com.dc.melodiasmario.core.commonui.biometric

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.presentation.biometric.AppLockController

@Composable
expect fun BindAppLockLifecycle(
    controller: AppLockController,
)