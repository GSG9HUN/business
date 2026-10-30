package com.dc.melodiasmario.core.network.di

import com.dc.melodiasmario.core.network.realtime.MobileRealtimeClient
import org.koin.core.annotation.ComponentScan
import org.koin.core.annotation.Module
import org.koin.core.annotation.Single

/**
 * Registers network dependencies with Koin.
 */
@Module
@ComponentScan("com.dc.melodiasmario.core.network")
class NetworkModule {
    /**
     * Provides the mobile realtime client operation for network module.
     */
    @Single
    fun mobileRealtimeClient(): MobileRealtimeClient = MobileRealtimeClient()
}
