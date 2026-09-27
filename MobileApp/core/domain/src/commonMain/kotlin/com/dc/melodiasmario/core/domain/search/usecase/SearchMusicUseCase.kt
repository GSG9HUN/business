package com.dc.melodiasmario.core.domain.search.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.domain.search.MusicSearchRepository
import com.dc.melodiasmario.core.model.search.MusicSearchPage
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class SearchMusicUseCase(
    private val musicSearchRepository: MusicSearchRepository
) {
    operator fun invoke(
        guildId: String,
        provider: TrackSearchPrefix,
        kind: MusicSearchResultKind,
        query: String,
        pageToken: String? = null
    ): Flow<Resource<MusicSearchPage>> =
        flow {
            emit(Resource.Loading)

            try {
                val result = musicSearchRepository.search(
                    guildId = guildId,
                    provider = provider,
                    kind = kind,
                    query = query,
                    pageToken = pageToken,
                )
                emit(Resource.Success(result))
            } catch (e: Exception) {
                emit(Resource.Error(e))
            }
        }
}