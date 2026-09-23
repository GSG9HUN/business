package com.dc.melodiasmario.core.network.queue.dto

import kotlinx.serialization.Serializable

@Serializable
data class EnqueueRequestDto(
    val query: String,
)
