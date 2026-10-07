package com.dc.melodiasmario.core.data.queue

import com.dc.melodiasmario.core.data.auth.AuthorizedSessionProvider
import com.dc.melodiasmario.core.domain.queue.QueueRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
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

    override suspend fun addToQueue(guildId: String, query: String): BotControlCommand {
        val accessToken = authorizedSessionProvider.getValidSession()
        return queueRemoteDataSource.addToQueue(
            accessToken = accessToken,
            guildId = guildId,
            query = query,
        )
    }

    override suspend fun removeFromQueue(guildId: String, trackNumber: String): BotControlCommand {
        val accessToken = authorizedSessionProvider.getValidSession()
        return queueRemoteDataSource.removeFromQueue(
            accessToken = accessToken,
            guildId = guildId,
            trackNumber = trackNumber,
        )
    }

    override suspend fun clearQueue(guildId: String): BotControlCommand {
        val accessToken = authorizedSessionProvider.getValidSession()
        return queueRemoteDataSource.clearQueue(
            accessToken = accessToken,
            guildId = guildId,
        )
    }

    override suspend fun shuffleQueue(guildId: String): BotControlCommand {
        val accessToken = authorizedSessionProvider.getValidSession()
        return queueRemoteDataSource.shuffleQueue(
            accessToken = accessToken,
            guildId = guildId,
        )
    }

    override suspend fun moveTrackToIndex(guildId: String, trackIndex: Int, targetIndex: Int): BotControlCommand {
        val accessToken = authorizedSessionProvider.getValidSession()
        return queueRemoteDataSource.moveTrackToIndex(
            accessToken = accessToken,
            guildId = guildId,
            trackIndex = trackIndex,
            targetIndex = targetIndex,
        )
    }
}
