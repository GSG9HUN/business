package com.dc.melodiasmario.feature.guild.domain.model

data class Guild (
    val id: String,
    val name: String,
    val iconUrl: String? = null,
    val accessLevel: GuildAccessLevel,
    val botStatus: BotStatus? = null,
)
