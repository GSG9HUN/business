package com.dc.melodiasmario.core.network.search

import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchPage
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind

interface MusicSearchRemoteDataSource {
    suspend fun getCapabilities(
        accessToken: String,
        guildId: String,
    ): List<MusicSearchCapability>

    suspend fun search(
        accessToken: String,
        guildId: String,
        provider: TrackSearchPrefix,
        kind: MusicSearchResultKind,
        query: String,
        pageToken: String?,
    ): MusicSearchPage
}