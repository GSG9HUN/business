package com.dc.melodiasmario

import android.app.Application
import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.di.androidAppModule
import com.dc.melodiasmario.shared.di.SharedModule
import org.koin.android.ext.koin.androidContext
import org.koin.core.context.startKoin
import com.dc.melodiasmario.shared.di.module

class MelodiasMario : Application() {
    override fun onCreate() {
        super.onCreate()

        startKoin {
            androidContext(this@MelodiasMario)

            properties(
                mapOf(
                    AppConstants.Properties.ApiBaseUrl to AppConstants.URLs.BaseUrl
                ),
            )

            modules(
                SharedModule().module(),
                androidAppModule,
            )
        }
    }
}
