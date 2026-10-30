package com.dc.melodiasmario.core.model.queue

import com.dc.melodiasmario.core.model.currenttrack.Track

/**
 * Represents queue shared between application layers.
 *
 * @property guildId Identifier of the guild owning this value.
 * @property trackCount Track count.
 * @property tracks Tracks.
 */
data class Queue(
    val guildId: String = "",
    val trackCount: Int = 0,
    val tracks: List<Track> = emptyList()
)
