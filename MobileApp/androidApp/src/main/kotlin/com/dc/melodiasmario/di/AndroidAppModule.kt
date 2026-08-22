package com.dc.melodiasmario.di

import com.dc.melodiasmario.core.settings.data.UserSettingsStorage
import com.dc.melodiasmario.core.settings.locale.AppLocaleController
import org.koin.android.ext.koin.androidContext
import org.koin.dsl.module

val androidAppModule = module {
    single {
        UserSettingsStorage(
            context = androidContext()
        )
    }

    single {
        AppLocaleController(
            context = androidContext()
        )
    }
}
