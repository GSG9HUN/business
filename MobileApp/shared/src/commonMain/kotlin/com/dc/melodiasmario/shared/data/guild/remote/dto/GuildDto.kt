package com.dc.melodiasmario.shared.data.guild.remote.dto

import com.dc.melodiasmario.shared.domain.guild.model.Guild
import com.dc.melodiasmario.shared.domain.guild.model.GuildAccessLevel
import kotlinx.serialization.Serializable

@Serializable
data class GuildDto(
    val guildId: Long,
    val name: String = "",
    val iconUrl: String? = null,
    val accessLevel: String,
    val botStatus: BotStatusDto
)

fun GuildDto.toDomain(): Guild {
    return Guild(
        id = guildId.toString(),
        name = name.ifBlank { guildId.toString() },
        iconUrl = iconUrl,
        accessLevel = accessLevel.toGuildAccessLevel(),
        botStatus = botStatus.toDomain(),
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