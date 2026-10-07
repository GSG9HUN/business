package com.dc.melodiasmario.core.domain.search

import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.model.search.MusicSearchPage
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind

interface MusicSearchRepository {
    suspend fun getCapabilities(guildId: String): List<MusicSearchCapability>

    suspend fun search(
        guildId: String,
        provider: TrackSearchPrefix,
        kind: MusicSearchResultKind,
        query: String,
        pageToken: String?,
    ): MusicSearchPage
}