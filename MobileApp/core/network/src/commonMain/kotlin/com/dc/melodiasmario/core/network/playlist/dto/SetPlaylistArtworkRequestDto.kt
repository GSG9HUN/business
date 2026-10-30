package com.dc.melodiasmario.core.network.playlist.dto

import kotlinx.serialization.Serializable

/** A null artwork URL restores automatic playlist artwork. */
@Serializable
data class SetPlaylistArtworkRequestDto(val artworkUri: String?)
