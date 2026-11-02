package com.dc.melodiasmario.core.model.realtime

import com.dc.melodiasmario.core.model.currenttrack.PlaybackStatus

/**
 * Defines the interactions handled by playback realtime presentation logic.
 *
 * @property guildId Identifier of the guild owning this value.
 * @property eventName Event name.
 * @property updatedAtUtc Updated at utc.
 * @property snapshot Snapshot.
 */
data class PlaybackRealtimeEvent(
    override val guildId: String,
    override val eventName: String,
    val updatedAtUtc: String,
    val snapshot: PlaybackStatus
) : RealtimeEvent
