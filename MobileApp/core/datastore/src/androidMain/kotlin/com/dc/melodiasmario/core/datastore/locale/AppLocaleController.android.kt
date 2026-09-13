package com.dc.melodiasmario.core.datastore.locale

import android.content.Context
import android.content.res.Configuration
import android.os.LocaleList
import java.util.Locale

actual class AppLocaleController(
    private val context: Context,
) {
    actual fun applyLocale(languageCode: String) {
        val locale = Locale.forLanguageTag(languageCode.takeIf { it.isNotBlank() } ?: DEFAULT_LANGUAGE_CODE)
        Locale.setDefault(locale)

        val configuration = Configuration(context.resources.configuration)
        configuration.setLocales(LocaleList(locale))
        context.resources.updateConfiguration(configuration, context.resources.displayMetrics)
    }

    private companion object {
        private const val DEFAULT_LANGUAGE_CODE = "en"
    }
}
