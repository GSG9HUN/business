package com.dc.melodiasmario.core.data.search

import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.data.auth.AuthorizedSessionProvider
import com.dc.melodiasmario.core.domain.search.MusicSearchRepository
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchPage
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind
import com.dc.melodiasmario.core.network.search.MusicSearchRemoteDataSource
import org.koin.core.annotation.Single

/**
 * Implements music search data access using the injected data sources.
 */
@Single(binds = [MusicSearchRepository::class])
class MusicSearchRepositoryImpl(
    private val musicSearchRemoteDataSource: MusicSearchRemoteDataSource,
    private val authorizedSessionProvider: AuthorizedSessionProvider
) : MusicSearchRepository {
    override suspend fun getCapabilities(guildId: String): List<MusicSearchCapability> {
        val accessToken = authorizedSessionProvider.getValidSession()
        return musicSearchRemoteDataSource.getCapabilities(
            accessToken = accessToken,
            guildId = guildId
        )
    }

    override suspend fun search(
        guildId: String,
        provider: TrackSearchPrefix,
        kind: MusicSearchResultKind,
        query: String,
        pageToken: String?
    ): MusicSearchPage {
        val accessToken = authorizedSessionProvider.getValidSession()
        return musicSearchRemoteDataSource.search(
            accessToken = accessToken,
            guildId = guildId,
            provider = provider,
            kind = kind,
            query = query,
            pageToken = pageToken
        )
    }
}
