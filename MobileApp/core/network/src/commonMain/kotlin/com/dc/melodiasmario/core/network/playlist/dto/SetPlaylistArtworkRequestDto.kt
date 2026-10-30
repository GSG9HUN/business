package com.dc.melodiasmario.core.network.playlist.dto

import kotlinx.serialization.Serializable

/**
 * A null artwork URL restores automatic playlist artwork.
 *
 * @property artworkUri Optional artwork URI returned by the API.
 */
@Serializable
data class SetPlaylistArtworkRequestDto(val artworkUri: String?)
