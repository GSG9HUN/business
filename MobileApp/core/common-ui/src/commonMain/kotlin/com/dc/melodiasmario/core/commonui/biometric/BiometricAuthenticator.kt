package com.dc.melodiasmario.core.commonui.biometric

import com.dc.melodiasmario.core.presentation.biometric.BiometricResult

/**
 * Provides biometric authenticator for the biometric layer.
 */
expect class BiometricAuthenticator {
    /**
     * Runs the platform biometric authentication flow and returns its outcome.
     */
    suspend fun authenticate(
        title: String,
        reason: String
    ): BiometricResult
}
