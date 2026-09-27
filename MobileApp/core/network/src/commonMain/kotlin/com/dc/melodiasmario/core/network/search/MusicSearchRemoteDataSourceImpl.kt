package com.dc.melodiasmario.core.network.search

import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchPage
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind
import org.koin.core.annotation.Single

@Single(binds = [MusicSearchRemoteDataSource::class])
class MusicSearchRemoteDataSourceImpl(
    private val musicSearchApiService: MusicSearchApiService
): MusicSearchRemoteDataSource {
    override suspend fun getCapabilities(
        accessToken: String,
        guildId: String
    ): List<MusicSearchCapability> {
        return musicSearchApiService.getCapabilities(accessToken = accessToken, guildId = guildId).map { it.toDomain() }
    }

    override suspend fun search(
        accessToken: String,
        guildId: String,
        provider: TrackSearchPrefix,
        kind: MusicSearchResultKind,
        query: String,
        pageToken: String?
    ): MusicSearchPage {
        return musicSearchApiService.search(
            accessToken = accessToken,
            guildId = guildId,
            provider = provider.toMusicSearchProviderId(),
            kind = kind.toApiKind(),
            query = query,
            pageToken = pageToken
        ).toDomain()
    }
}
