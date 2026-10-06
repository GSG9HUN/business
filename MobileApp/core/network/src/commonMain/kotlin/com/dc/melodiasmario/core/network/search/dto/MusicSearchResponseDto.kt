package com.dc.melodiasmario.core.network.search.dto

import com.dc.melodiasmario.core.model.search.MusicSearchPage
import kotlinx.serialization.Serializable

@Serializable
data class MusicSearchResponseDto(
    val results: List<MusicSearchResultDto>,
    val nextPageToken: String? = null,
){
    fun toDomain() = MusicSearchPage(
        results = results.mapNotNull { it.toDomainOrNull() },
        nextPageToken = nextPageToken
    )
}
