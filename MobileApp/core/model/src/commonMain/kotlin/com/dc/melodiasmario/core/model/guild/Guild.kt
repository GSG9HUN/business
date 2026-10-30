package com.dc.melodiasmario.core.model.guild

/**
 * Represents guild shared between application layers.
 *
 * @property id Id.
 * @property name Name.
 * @property iconUrl Icon url.
 * @property accessLevel Access level.
 * @property botStatus Bot status.
 */
data class Guild (
    val id: String,
    val name: String,
    val iconUrl: String? = null,
    val accessLevel: GuildAccessLevel,
    val botStatus: BotStatus? = null
)
