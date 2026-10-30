package com.dc.melodiasmario.core.network.search

import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchPage
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind

/**
 * Defines remote music search data operations.
 */
interface MusicSearchRemoteDataSource {
    /**
     * Retrieves the music-search capabilities available for the selected guild.
     */
    suspend fun getCapabilities(
        accessToken: String,
        guildId: String
    ): List<MusicSearchCapability>

    /**
     * Searches for music using the selected provider, result kind, query, and continuation token.
     */
    suspend fun search(
        accessToken: String,
        guildId: String,
        provider: TrackSearchPrefix,
        kind: MusicSearchResultKind,
        query: String,
        pageToken: String?
    ): MusicSearchPage
}
