package com.dc.melodiasmario.core.domain.search.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.search.MusicSearchRepository
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import kotlin.coroutines.cancellation.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes get music search capabilities as a domain operation.
 */
@Factory
class GetMusicSearchCapabilitiesUseCase(
    private val musicSearchRepository: MusicSearchRepository
) {
    /**
     * Executes the get music search capabilities domain operation.
     */
    operator fun invoke(guildId: String): Flow<Resource<List<MusicSearchCapability>>> = flow {
        emit(value = Resource.Loading)

        try {
            val capabilities = musicSearchRepository.getCapabilities(guildId = guildId)
            emit(value = Resource.Success(data = capabilities))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
