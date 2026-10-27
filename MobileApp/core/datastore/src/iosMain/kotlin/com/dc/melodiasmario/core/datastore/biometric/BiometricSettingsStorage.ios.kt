package com.dc.melodiasmario.core.datastore.biometric

import platform.Foundation.NSUserDefaults

actual class BiometricSettingsStorage {
    private val preferences = NSUserDefaults.standardUserDefaults

    actual suspend fun isEnabled(): Boolean = preferences.boolForKey(KEY_ENABLED)

    actual suspend fun setEnabled(enabled: Boolean) {
        preferences.setBool(enabled, forKey = KEY_ENABLED)
    }

    private companion object {
        const val KEY_ENABLED = "melodias_mario_biometric_lock_enabled"
    }
}