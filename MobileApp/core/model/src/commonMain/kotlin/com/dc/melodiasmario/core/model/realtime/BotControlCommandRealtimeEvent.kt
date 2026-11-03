package com.dc.melodiasmario.core.model.realtime

/**
 * Defines the interactions handled by bot control command realtime presentation logic.
 *
 * @property commandId Command id.
 * @property guildId Identifier of the guild owning this value.
 * @property userId User id.
 * @property type Type.
 * @property state State.
 * @property errorKey Error key.
 * @property errorMessage Error message.
 * @property resultJson Result json.
 * @property createdAtUtc Created at utc.
 * @property claimedAtUtc Claimed at utc.
 * @property completedAtUtc Completed at utc.
 * @property eventName Event name.
 */
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
    override val eventName: String
) : RealtimeEvent
