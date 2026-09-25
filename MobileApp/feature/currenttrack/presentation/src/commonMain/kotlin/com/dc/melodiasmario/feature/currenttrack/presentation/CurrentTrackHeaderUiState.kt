package com.dc.melodiasmario.feature.currenttrack.presentation

import com.dc.melodiasmario.core.model.guild.BotStatus

data class CurrentTrackHeaderUiState(
    val guildName: String = "",
    val guildIconUrl: String? = null,
    val guildBotStatus: BotStatus? = null,
    val profileName: String = "",
    val profileAvatarUrl: String? = null,
)
