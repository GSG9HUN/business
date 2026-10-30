package com.dc.melodiasmario.core.model.realtime

import com.dc.melodiasmario.core.model.guild.BotStatus

/**
 * Defines the interactions handled by guild bot status realtime presentation logic.
 *
 * @property guildId Identifier of the guild owning this value.
 * @property eventName Event name.
 * @property updatedAtUtc Updated at utc.
 * @property snapshot Snapshot.
 */
data class GuildBotStatusRealtimeEvent(
    override val guildId: String,
    override val eventName: String,
    val updatedAtUtc: String,
    val snapshot: BotStatus
) : RealtimeEvent
