package com.dc.melodiasmario.core.data.guild

import kotlin.test.Test

// TODO: Implement tests for GuildRepositoryImpl authorized session handling and remote delegation.
class GuildRepositoryImplTest {

    @Test
    fun getGuildsRequestsValidAccessTokenBeforeCallingRemoteDataSource() {
        // TODO: Given authorized session provider returns token, when getGuilds is called, then token is used remotely.
    }

    @Test
    fun getGuildsReturnsRemoteGuilds() {
        // TODO: Given remote data source returns guilds, when getGuilds is called, then same guilds are returned.
    }

    @Test
    fun getGuildsPropagatesSessionProviderFailure() {
        // TODO: Given authorized session provider fails, when getGuilds is called, then error is propagated.
    }
}
