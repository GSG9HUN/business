package com.dc.melodiasmario.core.network.realtime.dto

import com.dc.melodiasmario.core.model.realtime.GuildBotStatusRealtimeEvent
import com.dc.melodiasmario.core.network.guild.dto.BotStatusDto
import kotlinx.serialization.Serializable

/**
 * Represents guild bot status realtime event dto in the API transport contract.
 *
 * @property guildId Identifier of the guild owning this value.
 * @property eventName Event name.
 * @property updatedAtUtc Updated at utc.
 * @property snapshot Snapshot.
 */
@Serializable
data class GuildBotStatusRealtimeEventDto(
    val guildId: String,
    val eventName: String,
    val updatedAtUtc: String,
    val snapshot: BotStatusDto
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): GuildBotStatusRealtimeEvent = GuildBotStatusRealtimeEvent(
        guildId = guildId,
        eventName = eventName,
        updatedAtUtc = updatedAtUtc,
        snapshot = snapshot.toDomain()
    )
}
