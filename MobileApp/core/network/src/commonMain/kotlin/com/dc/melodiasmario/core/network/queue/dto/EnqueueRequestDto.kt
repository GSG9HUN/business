package com.dc.melodiasmario.core.network.queue.dto

import kotlinx.serialization.Serializable

/**
 * Represents enqueue request dto in the API transport contract.
 *
 * @property query Query.
 */
@Serializable
data class EnqueueRequestDto(
    val query: String
)
