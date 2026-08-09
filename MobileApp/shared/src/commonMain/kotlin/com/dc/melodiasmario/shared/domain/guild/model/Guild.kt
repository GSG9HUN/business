package com.dc.melodiasmario.shared.domain.guild.model

data class Guild (
    val id: String,
    val name: String,
    val iconUrl: String? = null,
    val accessLevel: GuildAccessLevel,
    val botStatus: BotStatus? = null,
)
