package com.dc.melodiasmario.core.datastore.locale

/**
 * Owns app locale coordination and its lifecycle-bound operations.
 */
expect class AppLocaleController {
    /**
     * Applies the selected language through the platform locale adapter.
     */
    fun applyLocale(languageCode: String): Unit
}
