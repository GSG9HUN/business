package com.dc.melodiasmario.core.network.queue

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.queue.Queue

/**
 * Defines remote queue data operations.
 */
interface QueueRemoteDataSource {
    /**
     * Retrieves the current playback queue for the requested guild.
     */
    suspend fun getQueue(accessToken: String, guildId: String): Queue

    /**
     * Requests adding the supplied music query or URL to the guild playback queue.
     */
    suspend fun addToQueue(accessToken: String, guildId: String, query: String): BotControlCommand

    /**
     * Requests removal of a track from the playback queue.
     */
    suspend fun removeFromQueue(accessToken: String, guildId: String, trackNumber: String): BotControlCommand

    /**
     * Requests clearing the guild playback queue.
     */
    suspend fun clearQueue(accessToken: String, guildId: String): BotControlCommand

    /**
     * Requests shuffling the guild playback queue.
     */
    suspend fun shuffleQueue(accessToken: String, guildId: String): BotControlCommand

    /**
     * Requests moving a queued track between zero-based queue indexes.
     */
    suspend fun moveTrackToIndex(accessToken: String, guildId: String, trackIndex: Int, targetIndex: Int): BotControlCommand
}
