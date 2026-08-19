package com.dc.melodiasmario.di

import com.dc.melodiasmario.core.settings.data.UserSettingsStorage
import org.koin.android.ext.koin.androidContext
import org.koin.dsl.module

val androidAppModule = module {
    single {
        UserSettingsStorage(
            context = androidContext()
        )
    }
}