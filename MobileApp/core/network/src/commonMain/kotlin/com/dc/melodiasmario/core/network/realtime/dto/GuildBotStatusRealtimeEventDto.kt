package com.dc.melodiasmario.core.network.realtime.dto

import com.dc.melodiasmario.core.model.realtime.GuildBotStatusRealtimeEvent
import com.dc.melodiasmario.core.network.guild.dto.BotStatusDto
import kotlinx.serialization.Serializable

@Serializable
data class GuildBotStatusRealtimeEventDto(
    val guildId: String,
    val eventName: String,
    val updatedAtUtc: String,
    val snapshot: BotStatusDto,
) {
    fun toDomain() = GuildBotStatusRealtimeEvent(
        guildId = guildId,
        eventName = eventName,
        updatedAtUtc = updatedAtUtc,
        snapshot = snapshot.toDomain()
    )
}
