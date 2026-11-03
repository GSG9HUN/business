package com.dc.melodiasmario.core.datastore.biometric

/**
 * Provides biometric settings storage for the biometric layer.
 */
expect class BiometricSettingsStorage {
    /**
     * Reports whether enabled.
     */
    suspend fun isEnabled(): Boolean

    /**
     * Persists the requested enabled setting.
     */
    suspend fun setEnabled(enabled: Boolean): Unit
}
