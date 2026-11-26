package com.dc.melodiasmario.core.model.audioeffect

/** Last confirmed settings and a guild-local page of custom presets; null settings require synchronization.
 * @property settings Confirmed sound, or null until the player has been synchronized.
 * @property presets Names on the current guild preset page.
 * @property canManagePresets Cached management permission, revalidated live by the bot.
 * @property page One-based page number.
 * @property pages Total number of pages, at least one.
 */
data class AudioEffectsSnapshot(
    val settings: AudioEffectsSettings?,
    val presets: List<String>,
    val canManagePresets: Boolean,
    val page: Int,
    val pages: Int
)
