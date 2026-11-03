package com.dc.melodiasmario.core.commonui.biometric

import androidx.biometric.BiometricManager
import androidx.biometric.BiometricPrompt
import androidx.core.content.ContextCompat
import androidx.fragment.app.FragmentActivity
import com.dc.melodiasmario.core.presentation.biometric.BiometricResult
import kotlin.coroutines.resume
import kotlinx.coroutines.suspendCancellableCoroutine

/**
 * Provides biometric authenticator for the biometric layer.
 */
actual class BiometricAuthenticator(
    private val activity: FragmentActivity
) {
    private var failedCounter = 0

    /**
     * Runs the platform biometric authentication flow and returns its outcome.
     */
    actual suspend fun authenticate(
        title: String,
        reason: String
    ): BiometricResult {
        val authenticators =
            BiometricManager.Authenticators.BIOMETRIC_WEAK or BiometricManager.Authenticators.DEVICE_CREDENTIAL

        val availability = BiometricManager.from(activity)
            .canAuthenticate(authenticators)

        if (availability != BiometricManager.BIOMETRIC_SUCCESS) {
            return BiometricResult.Unavailable
        }

        return suspendCancellableCoroutine { continuation ->
            val prompt = BiometricPrompt(
                activity,
                ContextCompat.getMainExecutor(activity),
                object : BiometricPrompt.AuthenticationCallback() {

                    override fun onAuthenticationSucceeded(result: BiometricPrompt.AuthenticationResult) {
                        if (continuation.isActive) {
                            continuation.resume(
                                value = BiometricResult.Success
                            )
                        }
                    }

                    override fun onAuthenticationError(errorCode: Int, errString: CharSequence) {
                        if (continuation.isActive) {
                            continuation.resume(value = BiometricResult.NotAuthenticated)
                        }
                    }

                    override fun onAuthenticationFailed() {
                        // Egy sikertelen próbálkozás nem zárja be
                        // az azonosítóablakot.

                        if (failedCounter == 3) {
                            //TODO bezárni
                        }

                        failedCounter++
                    }
                }
            )

            continuation.invokeOnCancellation {
                activity.runOnUiThread {
                    prompt.cancelAuthentication()
                }
            }

            val promptInfo =
                BiometricPrompt.PromptInfo
                    .Builder()
                    .setTitle(title)
                    .setSubtitle(reason)
                    .setAllowedAuthenticators(authenticators)
                    .build()

            prompt.authenticate(promptInfo)
        }
    }
}
