package com.dc.melodiasmario.core.model.realtime

import com.dc.melodiasmario.core.model.queue.Queue

/**
 * Defines the interactions handled by queue realtime presentation logic.
 *
 * @property guildId Identifier of the guild owning this value.
 * @property eventName Event name.
 * @property updatedAtUtc Updated at utc.
 * @property snapshot Snapshot.
 */
data class QueueRealtimeEvent(
    override val guildId: String,
    override val eventName: String,
    val updatedAtUtc: String,
    val snapshot: Queue
) : RealtimeEvent
