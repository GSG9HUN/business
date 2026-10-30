package com.dc.melodiasmario.core.domain.queue

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.queue.Queue

/**
 * Defines queue data operations independently of their concrete data sources.
 */
interface QueueRepository {
    /**
     * Retrieves the current playback queue for the requested guild.
     */
    suspend fun getQueue(guildId: String): Queue

    /**
     * Requests adding the supplied music query or URL to the guild playback queue.
     */
    suspend fun addToQueue(guildId: String, query: String): BotControlCommand

    /**
     * Requests removal of a track from the playback queue.
     */
    suspend fun removeFromQueue(guildId: String, trackNumber: String): BotControlCommand

    /**
     * Requests clearing the guild playback queue.
     */
    suspend fun clearQueue(guildId: String): BotControlCommand

    /**
     * Requests shuffling the guild playback queue.
     */
    suspend fun shuffleQueue(guildId: String): BotControlCommand

    /**
     * Requests moving a queued track between zero-based queue indexes.
     */
    suspend fun moveTrackToIndex(guildId: String, trackIndex: Int, targetIndex: Int): BotControlCommand
}
