package com.dc.melodiasmario.core.commonui.biometric

import com.dc.melodiasmario.core.presentation.biometric.BiometricResult
import kotlinx.cinterop.ExperimentalForeignApi
import kotlinx.coroutines.suspendCancellableCoroutine
import platform.LocalAuthentication.LAContext
import platform.LocalAuthentication.LAPolicyDeviceOwnerAuthentication
import kotlin.coroutines.resume

@OptIn(ExperimentalForeignApi::class)
actual class BiometricAuthenticator {

    actual suspend fun authenticate(
        title: String,
        reason: String,
    ): BiometricResult {
        val context = LAContext()

        val available = context.canEvaluatePolicy(LAPolicyDeviceOwnerAuthentication, null)

        if (!available) {
            return BiometricResult.Unavailable
        }

        return suspendCancellableCoroutine { continuation ->
            continuation.invokeOnCancellation {
                context.invalidate()
            }

            context.evaluatePolicy(LAPolicyDeviceOwnerAuthentication, reason
            ) { success, _ ->
                if (continuation.isActive) {
                    continuation.resume(
                        if (success) {
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