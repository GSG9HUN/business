package com.dc.melodiasmario.core.model.realtime

/**
 * Defines the interactions handled by realtime presentation logic.
 */
sealed interface RealtimeEvent {
    /**
     * Guild id exposed by realtime event.
     */
    val guildId: String

    /**
     * Event name exposed by realtime event.
     */
    val eventName: String
}
