package com.dc.melodiasmario.core.network.search.dto

import com.dc.melodiasmario.core.model.search.MusicSearchResult
import kotlinx.serialization.Serializable

/**
 * Represents music search result dto in the API transport contract.
 *
 * @property id Id.
 * @property providerId Provider id.
 * @property kind Kind.
 * @property title Title.
 * @property creator Creator.
 * @property thumbnailUrl Optional track artwork URL.
 * @property durationSeconds Track duration in seconds.
 * @property itemCount Item count.
 * @property canonicalUrl Canonical url.
 * @property canEnqueue Can enqueue.
 */
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
    val canEnqueue: Boolean = canonicalUrl != null
){
    /**
     * Maps this value to domain or null.
     */
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
