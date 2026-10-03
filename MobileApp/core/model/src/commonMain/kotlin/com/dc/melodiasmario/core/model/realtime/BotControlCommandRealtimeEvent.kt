package com.dc.melodiasmario.core.model.realtime

data class BotControlCommandRealtimeEvent(
    val commandId: String,
    override val guildId: String,
    val userId: String,
    val type: String,
    val state: String,
    val errorKey: String?,
    val errorMessage: String?,
    val resultJson: String?,
    val createdAtUtc: String,
    val claimedAtUtc: String?,
    val completedAtUtc: String?,
    override val eventName: String,
) : RealtimeEvent
