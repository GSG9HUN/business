package com.dc.melodiasmario.core.model.playlist

/**
 * Represents guild data shared between application layers.
 *
 * @property avatarUrl Avatar url.
 * @property name Name.
 */
data class GuildData(
    val avatarUrl: String? = null,
    val name: String = ""
)
