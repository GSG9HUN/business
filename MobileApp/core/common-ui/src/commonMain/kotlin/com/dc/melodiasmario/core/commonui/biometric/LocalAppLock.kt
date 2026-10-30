package com.dc.melodiasmario.core.commonui.biometric

import androidx.compose.runtime.ProvidableCompositionLocal
import androidx.compose.runtime.staticCompositionLocalOf
import com.dc.melodiasmario.core.presentation.biometric.AppLockController

/**
 * Local app lock controller exposed by this module.
 */
val localAppLockController: ProvidableCompositionLocal<AppLockController> = staticCompositionLocalOf<AppLockController> {
    error(message = "AppLockController was not provided.")
}

/**
 * Local biometric authenticator exposed by this module.
 */
val localBiometricAuthenticator: ProvidableCompositionLocal<BiometricAuthenticator> =
    staticCompositionLocalOf<BiometricAuthenticator> {
    error(message = "BiometricAuthenticator was not provided.")
}
