package com.dc.melodiasmario.core.domain.queue

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.queue.Queue

interface QueueRepository {
    suspend fun getQueue(guildId: String): Queue
    suspend fun addToQueue(guildId: String, query: String): BotControlCommand
    suspend fun removeFromQueue(guildId: String, trackNumber: String): BotControlCommand
    suspend fun clearQueue(guildId: String): BotControlCommand
    suspend fun shuffleQueue(guildId: String): BotControlCommand
    suspend fun moveTrackToIndex(guildId: String, trackIndex: Int, targetIndex: Int): BotControlCommand
}
