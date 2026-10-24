package com.dc.melodiasmario.core.domain.search.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.search.MusicSearchRepository
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory
import kotlin.coroutines.cancellation.CancellationException

@Factory
class GetMusicSearchCapabilitiesUseCase(
    private val musicSearchRepository: MusicSearchRepository
) {
    operator fun invoke(guildId: String): Flow<Resource<List<MusicSearchCapability>>> = flow {
        emit(Resource.Loading)

        try {
            val capabilities = musicSearchRepository.getCapabilities(guildId)
            emit(Resource.Success(capabilities))
        } catch (e: CancellationException) {
            throw e
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}