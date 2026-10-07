package com.dc.melodiasmario.core.model.realtime

import com.dc.melodiasmario.core.model.guild.BotStatus

data class GuildBotStatusRealtimeEvent(
    override val guildId: String,
    override val eventName: String,
    val updatedAtUtc: String,
    val snapshot: BotStatus,
) : RealtimeEvent
