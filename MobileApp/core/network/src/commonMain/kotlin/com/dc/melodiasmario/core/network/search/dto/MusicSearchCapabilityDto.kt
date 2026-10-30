package com.dc.melodiasmario.core.network.search.dto

import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import kotlinx.serialization.Serializable

/**
 * Represents music search capability dto in the API transport contract.
 *
 * @property providerId Provider id.
 * @property supportsTrackSearch Supports track search.
 * @property supportsPlaylistSearch Supports playlist search.
 * @property canEnqueue Can enqueue.
 * @property unavailableReason Unavailable reason.
 */
@Serializable
data class MusicSearchCapabilityDto(
    val providerId: String,
    val supportsTrackSearch: Boolean = false,
    val supportsPlaylistSearch: Boolean = false,
    val canEnqueue: Boolean = false,
    val unavailableReason: String? = null
){
    /**
     * Maps this value to domain.
     */
    fun toDomain(): MusicSearchCapability = MusicSearchCapability(
        provider = providerId.toTrackSearchPrefixOrNull(),
        supportsTrackSearch = supportsTrackSearch,
        supportsPlaylistSearch = supportsPlaylistSearch,
        canEnqueue = canEnqueue,
        unavailableReason = unavailableReason
    )
}
