package com.dc.melodiasmario.core.commonui.biometric

import androidx.compose.runtime.staticCompositionLocalOf
import com.dc.melodiasmario.core.presentation.biometric.AppLockController

val LocalAppLockController = staticCompositionLocalOf<AppLockController> {
    error("AppLockController was not provided.")
}

val LocalBiometricAuthenticator = staticCompositionLocalOf<BiometricAuthenticator> {
    error("BiometricAuthenticator was not provided.")
}