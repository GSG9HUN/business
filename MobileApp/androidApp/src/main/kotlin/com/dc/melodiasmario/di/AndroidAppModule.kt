package com.dc.melodiasmario.di

import com.dc.melodiasmario.core.datastore.locale.AppLocaleController
import org.koin.android.ext.koin.androidContext
import org.koin.dsl.module

val androidAppModule = module {
    single {
        AppLocaleController(
            context = androidContext()
        )
    }
}
