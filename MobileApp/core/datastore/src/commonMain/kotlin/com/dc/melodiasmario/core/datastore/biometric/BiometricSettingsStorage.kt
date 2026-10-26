package com.dc.melodiasmario.core.datastore.biometric

expect class BiometricSettingsStorage {
    suspend fun isEnabled(): Boolean

    suspend fun setEnabled(enabled: Boolean)
}