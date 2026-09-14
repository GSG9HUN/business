package com.dc.melodiasmario.core.model.guild

data class Guild (
    val id: String,
    val name: String,
    val iconUrl: String? = null,
    val accessLevel: GuildAccessLevel,
    val botStatus: BotStatus? = null,
)
