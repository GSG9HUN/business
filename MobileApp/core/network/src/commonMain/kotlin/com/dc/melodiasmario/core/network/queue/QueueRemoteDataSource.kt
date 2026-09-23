package com.dc.melodiasmario.core.network.queue

import com.dc.melodiasmario.core.model.queue.Queue

interface QueueRemoteDataSource {
    suspend fun getQueue(accessToken: String, guildId: String): Queue
    suspend fun addToQueue(accessToken: String, guildId: String, query: String)
    suspend fun removeFromQueue(accessToken: String, guildId: String, trackNumber: String)
    suspend fun clearQueue(accessToken: String, guildId: String)
    suspend fun shuffleQueue(accessToken: String, guildId: String)
    suspend fun moveTrackUp(accessToken: String, guildId: String, trackIndex: Int)
    suspend fun moveTrackDown(accessToken: String, guildId: String, trackIndex: Int)
}
