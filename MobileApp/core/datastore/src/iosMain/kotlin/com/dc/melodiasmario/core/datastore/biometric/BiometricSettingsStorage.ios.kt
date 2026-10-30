package com.dc.melodiasmario.core.datastore.biometric

import platform.Foundation.NSUserDefaults

/**
 * Provides biometric settings storage for the biometric layer.
 */
actual class BiometricSettingsStorage {
    private val preferences = NSUserDefaults.standardUserDefaults

    /**
     * Reports whether enabled.
     */
    actual suspend fun isEnabled(): Boolean = preferences.boolForKey(KEY_ENABLED)

    /**
     * Persists the requested enabled setting.
     */
    actual suspend fun setEnabled(enabled: Boolean): Unit {
        preferences.setBool(enabled, forKey = KEY_ENABLED)
    }

    private companion object {
        const val KEY_ENABLED = "melodias_mario_biometric_lock_enabled"
    }
}
