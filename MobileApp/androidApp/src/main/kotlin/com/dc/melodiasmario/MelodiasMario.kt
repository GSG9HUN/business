package com.dc.melodiasmario

import android.app.Application
import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.di.androidAppModule
import com.dc.melodiasmario.shared.di.SharedModule
import com.dc.melodiasmario.shared.di.module
import org.koin.android.ext.koin.androidContext
import org.koin.core.context.startKoin

/**
 * Provides melodias mario for the melodiasmario layer.
 */
class MelodiasMario : Application() {
    override fun onCreate(): Unit {
        super.onCreate()

        startKoin {
            androidContext(this@MelodiasMario)

            properties(
                mapOf(
                    AppConstants.Properties.ApiBaseUrl to AppConstants.URLs.BaseUrl
                )
            )

            modules(
                SharedModule().module(),
                androidAppModule
            )
        }
    }
}
