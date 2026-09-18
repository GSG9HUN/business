package com.dc.melodiasmario.core.data.queue

import com.dc.melodiasmario.core.data.auth.AuthorizedSessionProvider
import com.dc.melodiasmario.core.domain.queue.QueueRepository
import com.dc.melodiasmario.core.model.queue.Queue
import com.dc.melodiasmario.core.network.queue.QueueRemoteDataSource
import org.koin.core.annotation.Single

@Single(binds = [QueueRepository::class])
class QueueRepositoryImpl(
    private val queueRemoteDataSource: QueueRemoteDataSource,
    private val authorizedSessionProvider: AuthorizedSessionProvider
): QueueRepository {

    override suspend fun getQueue(guildId: String): Queue {
        val accessToken = authorizedSessionProvider.getValidSession()
        return queueRemoteDataSource.getQueue(
            accessToken = accessToken,
            guildId = guildId,
        )
    }

    override suspend fun addToQueue(guildId: String, query: String) {
        val accessToken = authorizedSessionProvider.getValidSession()
        queueRemoteDataSource.addToQueue(
            accessToken = accessToken,
            guildId = guildId,
            query = query,
        )
    }

    override suspend fun removeFromQueue(guildId: String, trackNumber: String) {
        val accessToken = authorizedSessionProvider.getValidSession()
        queueRemoteDataSource.removeFromQueue(
            accessToken = accessToken,
            guildId = guildId,
            trackNumber = trackNumber,
        )
    }

    override suspend fun clearQueue(guildId: String) {
        val accessToken = authorizedSessionProvider.getValidSession()
        queueRemoteDataSource.clearQueue(
            accessToken = accessToken,
            guildId = guildId,
        )
    }

    override suspend fun shuffleQueue(guildId: String) {
        val accessToken = authorizedSessionProvider.getValidSession()
        queueRemoteDataSource.shuffleQueue(
            accessToken = accessToken,
            guildId = guildId,
        )
    }

    override suspend fun moveTrackUp(guildId: String, trackIndex: Int) {
        val accessToken = authorizedSessionProvider.getValidSession()
        queueRemoteDataSource.moveTrackUp(
            accessToken = accessToken,
            guildId = guildId,
            trackIndex = trackIndex,
        )
    }

    override suspend fun moveTrackDown(guildId: String, trackIndex: Int) {
        val accessToken = authorizedSessionProvider.getValidSession()
        queueRemoteDataSource.moveTrackDown(
            accessToken = accessToken,
            guildId = guildId,
            trackIndex = trackIndex,
        )
    }
}
