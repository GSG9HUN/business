package com.dc.melodiasmario.core.network.queue

import com.dc.melodiasmario.core.model.queue.Queue
import org.koin.core.annotation.Single

@Single(binds = [QueueRemoteDataSource::class])
class QueueRemoteDataSourceImpl(
    private val queueApiService: QueueApiService
) : QueueRemoteDataSource {
    override suspend fun getQueue(accessToken: String, guildId: String): Queue {
        return queueApiService.getQueue(accessToken = accessToken, guildId = guildId).toDomain()
    }

    override suspend fun addToQueue(accessToken: String, guildId: String, query: String) {
        return queueApiService.addToQueue(
            accessToken = accessToken,
            guildId = guildId,
            query = query,
        )
    }

    override suspend fun removeFromQueue(accessToken: String, guildId: String, trackNumber: String) {
        return queueApiService.removeFromQueue(
            accessToken = accessToken,
            guildId = guildId,
            trackNumber = trackNumber,
        )
    }

    override suspend fun clearQueue(accessToken: String, guildId: String) {
        return queueApiService.clearQueue(accessToken = accessToken, guildId = guildId)
    }

    override suspend fun shuffleQueue(accessToken: String, guildId: String) {
        return queueApiService.shuffleQueue(accessToken = accessToken, guildId = guildId)
    }

    override suspend fun moveTrackUp(accessToken: String, guildId: String, trackIndex: Int) {
        return queueApiService.moveTrackUp(
            accessToken = accessToken,
            guildId = guildId,
            trackIndex = trackIndex,
        )
    }

    override suspend fun moveTrackDown(accessToken: String, guildId: String, trackIndex: Int) {
        return queueApiService.moveTrackDown(
            accessToken = accessToken,
            guildId = guildId,
            trackIndex = trackIndex,
        )
    }
}
