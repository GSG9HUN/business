package com.dc.melodiasmario.core.datastore.locale

import android.content.Context
import android.content.res.Configuration
import android.os.LocaleList
import java.util.Locale

/**
 * Owns app locale coordination and its lifecycle-bound operations.
 */
actual class AppLocaleController(
    private val context: Context
) {
    /**
     * Applies the selected language through the platform locale adapter.
     */
    actual fun applyLocale(languageCode: String): Unit {
        val locale = Locale.forLanguageTag(languageCode.takeIf { textValue -> textValue.isNotBlank() } ?: DEFAULT_LANGUAGE_CODE)
        Locale.setDefault(locale)

        val configuration = Configuration(context.resources.configuration)
        configuration.setLocales(LocaleList(locale))
        context.resources.updateConfiguration(configuration, context.resources.displayMetrics)
    }

    private companion object {
        private const val DEFAULT_LANGUAGE_CODE = "en"
    }
}
