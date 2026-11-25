package com.dc.melodiasmario.core.network.currenttrack.dto

import kotlinx.serialization.Serializable

/**
 * Submits a single absolute seek for the playback attempt captured at gesture start.
 *
 * @property positionSeconds Nonnegative position strictly before the end of the track.
 * @property expectedPlaybackOperationId Attempt identity returned by the playback snapshot.
 */
@Serializable
data class SeekPlaybackRequestDto(
    val positionSeconds: Int,
    val expectedPlaybackOperationId: String
)
