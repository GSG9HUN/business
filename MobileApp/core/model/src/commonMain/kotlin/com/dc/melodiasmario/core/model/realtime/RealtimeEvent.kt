package com.dc.melodiasmario.core.model.realtime

sealed interface RealtimeEvent {
    val guildId: String
    val eventName: String
}
