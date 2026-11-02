package com.dc.melodiasmario.core.network.guild.dto

import com.dc.melodiasmario.core.model.guild.Guild
import com.dc.melodiasmario.core.model.guild.GuildAccessLevel
import kotlinx.serialization.Serializable

/**
 * Represents guild dto in the API transport contract.
 *
 * @property guildId Identifier of the guild owning this value.
 * @property name Name.
 * @property iconUrl Icon url.
 * @property accessLevel Access level.
 * @property botStatus Bot status.
 */
@Serializable
data class GuildDto(
    val guildId: Long,
    val name: String = "",
    val iconUrl: String? = null,
    val accessLevel: String,
    val botStatus: BotStatusDto
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): Guild = Guild(
        id = guildId.toString(),
        name = name.ifBlank { guildId.toString() },
        iconUrl = iconUrl,
        accessLevel = accessLevel.toGuildAccessLevel(),
        botStatus = botStatus.toDomain()
    )
}

private fun String.toGuildAccessLevel(): GuildAccessLevel {
    return when (this.lowercase()) {
        "admin" -> GuildAccessLevel.Admin
        "member" -> GuildAccessLevel.ReadOnly
        "dj" -> GuildAccessLevel.DJ
        else -> this
    } as GuildAccessLevel
}
