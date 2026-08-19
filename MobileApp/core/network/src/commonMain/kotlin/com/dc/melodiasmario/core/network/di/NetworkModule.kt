package com.dc.melodiasmario.core.network.di

import com.dc.melodiasmario.core.network.client.createHttpClient
import io.ktor.client.HttpClient
import org.koin.core.annotation.ComponentScan
import org.koin.core.annotation.Module
import org.koin.core.annotation.Single

@Module
@ComponentScan("com.dc.melodiasmario.core.network")
class NetworkModule {
    @Single
    fun httpClient(): HttpClient = createHttpClient()
}
