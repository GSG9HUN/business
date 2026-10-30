package com.dc.melodiasmario.core.datastore.locale

/**
 * Owns app locale coordination and its lifecycle-bound operations.
 */
actual class AppLocaleController {
    /**
     * Applies the selected language through the platform locale adapter.
     */
    actual fun applyLocale(languageCode: String): Unit {
        // iOS app language handling needs native app-level integration.
    }
}
