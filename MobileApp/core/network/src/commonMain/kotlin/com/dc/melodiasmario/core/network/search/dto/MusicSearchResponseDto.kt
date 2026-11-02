package com.dc.melodiasmario.core.network.search.dto

import com.dc.melodiasmario.core.model.search.MusicSearchPage
import kotlinx.serialization.Serializable

/**
 * Represents music search response dto in the API transport contract.
 *
 * @property results Results.
 * @property nextPageToken Next page token.
 */
@Serializable
data class MusicSearchResponseDto(
    val results: List<MusicSearchResultDto>,
    val nextPageToken: String? = null
){
    /**
     * Maps this value to domain.
     */
    fun toDomain(): MusicSearchPage = MusicSearchPage(
        results = results.mapNotNull { searchResult -> searchResult.toDomainOrNull() },
        nextPageToken = nextPageToken
    )
}
