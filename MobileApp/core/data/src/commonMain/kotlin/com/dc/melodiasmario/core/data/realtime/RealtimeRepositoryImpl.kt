package com.dc.melodiasmario.core.data.realtime

import com.dc.melodiasmario.core.data.auth.AuthorizedSessionProvider
import com.dc.melodiasmario.core.domain.realtime.RealtimeRepository
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import com.dc.melodiasmario.core.network.realtime.MobileRealtimeClient
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single(binds = [RealtimeRepository::class])
class RealtimeRepositoryImpl(
    private val authorizedSessionProvider: AuthorizedSessionProvider,
    private val mobileRealtimeClient: MobileRealtimeClient,
) : RealtimeRepository {
    override val connectionState = mobileRealtimeClient.connectionState

    override fun observeGuildRealtime(guildId: String): Flow<RealtimeEvent> = flow {
        val accessToken = authorizedSessionProvider.getValidSession()

        mobileRealtimeClient
            .observeGuild(accessToken, guildId)
            .collect { event ->
                emit(event)
            }
    }

    override suspend fun disconnect() {
        mobileRealtimeClient.disconnect()
    }
}