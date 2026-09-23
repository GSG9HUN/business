package com.dc.melodiasmario.core.model.queue

import com.dc.melodiasmario.core.model.currenttrack.Track

data class Queue(
    val guildId: String = "",
    val trackCount: Int = 0,
    val tracks: List<Track> = emptyList(),
)