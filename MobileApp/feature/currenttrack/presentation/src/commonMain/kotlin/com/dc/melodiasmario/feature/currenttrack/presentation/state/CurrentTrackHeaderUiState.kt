package com.dc.melodiasmario.feature.currenttrack.presentation.state

import com.dc.melodiasmario.core.model.guild.BotStatus

/**
 * Defines current track header presentation state shared with UI consumers.
 *
 * @property guildName Guild name.
 * @property guildIconUrl Guild icon url.
 * @property guildBotStatus Guild bot status.
 * @property profileName Profile name.
 * @property profileAvatarUrl Profile avatar url.
 */
data class CurrentTrackHeaderUiState(
    val guildName: String = "",
    val guildIconUrl: String? = null,
    val guildBotStatus: BotStatus? = null,
    val profileName: String = "",
    val profileAvatarUrl: String? = null
)
