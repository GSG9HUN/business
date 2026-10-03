package com.dc.melodiasmario.core.network.queue

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.queue.Queue

interface QueueRemoteDataSource {
    suspend fun getQueue(accessToken: String, guildId: String): Queue
    suspend fun addToQueue(accessToken: String, guildId: String, query: String): BotControlCommand
    suspend fun removeFromQueue(accessToken: String, guildId: String, trackNumber: String): BotControlCommand
    suspend fun clearQueue(accessToken: String, guildId: String): BotControlCommand
    suspend fun shuffleQueue(accessToken: String, guildId: String): BotControlCommand
    suspend fun moveTrackUp(accessToken: String, guildId: String, trackIndex: Int): BotControlCommand
    suspend fun moveTrackDown(accessToken: String, guildId: String, trackIndex: Int): BotControlCommand
}
