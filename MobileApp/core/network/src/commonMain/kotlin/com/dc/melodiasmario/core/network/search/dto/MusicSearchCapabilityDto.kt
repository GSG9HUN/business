package com.dc.melodiasmario.core.network.search.dto

import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import kotlinx.serialization.Serializable

@Serializable
data class MusicSearchCapabilityDto(
    val providerId: String,
    val supportsTrackSearch: Boolean = false,
    val supportsPlaylistSearch: Boolean = false,
    val canEnqueue: Boolean = false,
    val unavailableReason: String? = null,
){
    fun toDomain() = MusicSearchCapability(
        provider = providerId.toTrackSearchPrefixOrNull(),
        supportsTrackSearch = supportsTrackSearch,
        supportsPlaylistSearch = supportsPlaylistSearch,
        canEnqueue = canEnqueue,
        unavailableReason = unavailableReason
    )
}
