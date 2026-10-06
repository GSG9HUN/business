package com.dc.melodiasmario.core.model.realtime

import com.dc.melodiasmario.core.model.queue.Queue

data class QueueRealtimeEvent(
    override val guildId: String,
    override val eventName: String,
    val updatedAtUtc: String,
    val snapshot: Queue,
) : RealtimeEvent
