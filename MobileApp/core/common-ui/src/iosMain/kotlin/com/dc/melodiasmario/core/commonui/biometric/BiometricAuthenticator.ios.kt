package com.dc.melodiasmario.core.commonui.biometric

import com.dc.melodiasmario.core.presentation.biometric.BiometricResult
import kotlin.coroutines.resume
import kotlinx.cinterop.ExperimentalForeignApi
import kotlinx.coroutines.suspendCancellableCoroutine
import platform.LocalAuthentication.LAContext
import platform.LocalAuthentication.LAPolicyDeviceOwnerAuthentication

/**
 * Provides biometric authenticator for the biometric layer.
 */
@OptIn(ExperimentalForeignApi::class)
actual class BiometricAuthenticator {

    /**
     * Runs the platform biometric authentication flow and returns its outcome.
     */
    actual suspend fun authenticate(
        title: String,
        reason: String
    ): BiometricResult {
        val lAContext = LAContext()

        val available = lAContext.canEvaluatePolicy(LAPolicyDeviceOwnerAuthentication, null)

        if (!available) {
            return BiometricResult.Unavailable
        }

        return suspendCancellableCoroutine { continuation ->
            continuation.invokeOnCancellation {
                lAContext.invalidate()
            }

            lAContext.evaluatePolicy(
                LAPolicyDeviceOwnerAuthentication,
                reason
            ) { success, _ ->

                if (continuation.isActive) {
                    continuation.resume(
                        value = if (success) {
                            BiometricResult.Success
                        } else {
                            BiometricResult.NotAuthenticated
                        }
                    )
                }
            }
        }
    }
}
