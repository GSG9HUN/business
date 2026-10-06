package com.dc.melodiasmario.core.network.search.dto

import com.dc.melodiasmario.core.model.search.MusicSearchResult
import kotlinx.serialization.Serializable

@Serializable
data class MusicSearchResultDto(
    val id: String,
    val providerId: String,
    val kind: String,
    val title: String,
    val creator: String? = null,
    val thumbnailUrl: String? = null,
    val durationSeconds: Int? = null,
    val itemCount: Int? = null,
    val canonicalUrl: String? = null,
    val canEnqueue: Boolean = canonicalUrl != null,
){
    fun toDomainOrNull(): MusicSearchResult? {
        val provider = providerId.toTrackSearchPrefixOrNull() ?: return null
        val resultKind = kind.toMusicSearchResultKindOrNull() ?: return null

        return MusicSearchResult(
            id = id,
            provider = provider,
            kind = resultKind,
            title = title,
            creator = creator,
            thumbnailUrl = thumbnailUrl,
            durationSeconds = durationSeconds,
            itemCount = itemCount,
            canonicalUrl = canonicalUrl,
            canEnqueue = canEnqueue
        )
    }
}
