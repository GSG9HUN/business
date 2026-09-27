package com.dc.melodiasmario.core.network.playlist.dto

import kotlinx.serialization.Serializable

@Serializable
data class RenamePlaylistRequestDto(
    val newName: String
)
