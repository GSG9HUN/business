package com.dc.melodiasmario.core.datastore.biometric

import android.content.Context
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

/**
 * Provides biometric settings storage for the biometric layer.
 */
actual class BiometricSettingsStorage(
    context: Context
) {
    private val preferences = context.applicationContext.getSharedPreferences(
        "biometric_settings",
        Context.MODE_PRIVATE
    )

    /**
     * Reports whether enabled.
     */
    actual suspend fun isEnabled(): Boolean = withContext(context = Dispatchers.IO) {
        preferences.getBoolean(KEY_ENABLED, false)
    }

    /**
     * Persists the requested enabled setting.
     */
    actual suspend fun setEnabled(enabled: Boolean): Unit {
        withContext(context = Dispatchers.IO) {
            check(
                value = preferences.edit()
                    .putBoolean(KEY_ENABLED, enabled)
                    .commit()
            ) {
                "Failed to persist biometric lock settings."
            }
        }
    }

    private companion object {
        const val KEY_ENABLED = "enabled"
    }
}
