package com.dc.melodiasmario.core.domain.search.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.domain.search.MusicSearchRepository
import com.dc.melodiasmario.core.model.search.MusicSearchPage
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind
import kotlin.coroutines.cancellation.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes search music as a domain operation.
 */
@Factory
class SearchMusicUseCase(
    private val musicSearchRepository: MusicSearchRepository
) {
    /**
     * Executes the search music domain operation.
     */
    operator fun invoke(
        guildId: String,
        provider: TrackSearchPrefix,
        kind: MusicSearchResultKind,
        query: String,
        pageToken: String? = null
    ): Flow<Resource<MusicSearchPage>> =
        flow {
            emit(value = Resource.Loading)

            try {
                val result = musicSearchRepository.search(
                    guildId = guildId,
                    provider = provider,
                    kind = kind,
                    query = query,
                    pageToken = pageToken
                )
                emit(value = Resource.Success(data = result))
            } catch (cancellationException: CancellationException) {
                throw cancellationException
            } catch (exception: Exception) {
                emit(value = Resource.Error(error = exception))
            }
        }
}
