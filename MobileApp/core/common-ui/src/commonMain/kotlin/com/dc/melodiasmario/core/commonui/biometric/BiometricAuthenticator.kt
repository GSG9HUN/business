package com.dc.melodiasmario.core.commonui.biometric

import com.dc.melodiasmario.core.presentation.biometric.BiometricResult

expect class BiometricAuthenticator {
    suspend fun authenticate(
        title: String,
        reason: String,
    ): BiometricResult
}