package com.dc.melodiasmario.core.di

import com.dc.melodiasmario.core.data.di.DataModule
import com.dc.melodiasmario.core.datastore.di.DatastoreModule
import com.dc.melodiasmario.core.domain.di.DomainModule
import com.dc.melodiasmario.core.network.client.createHttpClient
import com.dc.melodiasmario.core.network.di.NetworkModule
import io.ktor.client.HttpClient
import org.koin.core.annotation.Module
import org.koin.core.annotation.Single

/**
 * Registers core dependencies with Koin.
 */
@Module(
    includes = [
        DomainModule::class,
        DataModule::class,
        NetworkModule::class,
        DatastoreModule::class
    ]
)
class CoreModule {
    /**
     * Provides the http client operation for core module.
     */
    @Single
    fun httpClient(): HttpClient = createHttpClient()
}
