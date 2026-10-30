package com.dc.melodiasmario.core.domain.search

import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchPage
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind

/**
 * Defines music search data operations independently of their concrete data sources.
 */
interface MusicSearchRepository {
    /**
     * Retrieves the music-search capabilities available for the selected guild.
     */
    suspend fun getCapabilities(guildId: String): List<MusicSearchCapability>

    /**
     * Searches for music using the selected provider, result kind, query, and continuation token.
     */
    suspend fun search(
        guildId: String,
        provider: TrackSearchPrefix,
        kind: MusicSearchResultKind,
        query: String,
        pageToken: String?
    ): MusicSearchPage
}
