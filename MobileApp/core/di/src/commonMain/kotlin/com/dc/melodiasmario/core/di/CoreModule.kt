package com.dc.melodiasmario.core.di

import com.dc.melodiasmario.core.data.di.DataModule
import com.dc.melodiasmario.core.datastore.di.DatastoreModule
import com.dc.melodiasmario.core.domain.di.DomainModule
import com.dc.melodiasmario.core.network.di.NetworkModule
import com.dc.melodiasmario.core.network.client.createHttpClient
import io.ktor.client.HttpClient
import org.koin.core.annotation.Module
import org.koin.core.annotation.Single

@Module(
    includes = [
        DomainModule::class,
        DataModule::class,
        NetworkModule::class,
        DatastoreModule::class,
    ],
)
class CoreModule {
    @Single
    fun httpClient(): HttpClient = createHttpClient()
}
