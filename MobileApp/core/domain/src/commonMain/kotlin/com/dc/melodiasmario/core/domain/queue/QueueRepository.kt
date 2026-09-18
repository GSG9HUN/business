package com.dc.melodiasmario.core.domain.queue

import com.dc.melodiasmario.core.model.queue.Queue

interface QueueRepository {
    suspend fun getQueue(guildId: String): Queue
    suspend fun addToQueue(guildId: String, query: String)
    suspend fun removeFromQueue(guildId: String, trackNumber: String)
    suspend fun clearQueue(guildId: String)
    suspend fun shuffleQueue(guildId: String)
    suspend fun moveTrackUp(guildId: String, trackIndex: Int)
    suspend fun moveTrackDown(guildId: String, trackIndex: Int)
}
