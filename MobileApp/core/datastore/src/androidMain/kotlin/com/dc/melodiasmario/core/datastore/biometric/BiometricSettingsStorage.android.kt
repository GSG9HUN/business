package com.dc.melodiasmario.core.datastore.biometric

import android.content.Context
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

actual class BiometricSettingsStorage(
    context: Context,
) {
    private val preferences = context.applicationContext.getSharedPreferences(
        "biometric_settings",
        Context.MODE_PRIVATE,
    )

    actual suspend fun isEnabled(): Boolean = withContext(Dispatchers.IO) {
        preferences.getBoolean(KEY_ENABLED, false)
    }

    actual suspend fun setEnabled(enabled: Boolean) {
        withContext(Dispatchers.IO) {
            check(
                preferences.edit()
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
