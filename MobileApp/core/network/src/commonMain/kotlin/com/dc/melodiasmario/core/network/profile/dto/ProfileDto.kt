package com.dc.melodiasmario.core.network.profile.dto

import com.dc.melodiasmario.core.model.profile.ProfileData
import com.dc.melodiasmario.core.model.profile.ProfileSettingsData
import com.dc.melodiasmario.core.model.profile.ProfileUser
import com.dc.melodiasmario.core.model.settings.UserSettings
import kotlinx.serialization.Serializable

@Serializable
data class ProfileDto(
    val user: ProfileUserDto,
    val userSettings: ProfileSettingsDto,
)

@Serializable
data class ProfileUserDto(
    val discordUserId: String,
    val username: String,
    val displayName: String,
    val avatarUrl: String?,
    val isDiscordConnected: Boolean,
    val isActive: Boolean,
)

@Serializable
data class ProfileSettingsDto(
    val languageCode: String,
    val theme: String,
    val hapticFeedbackEnabled: Boolean,
    val soundEffectsEnabled: Boolean,
    val telemetryEnabled: Boolean,
    val updatedAtUtc: String,
)

fun ProfileUserDto.toDomain() = ProfileUser(
    id = discordUserId,
    displayName = displayName,
    username = username,
    provider = "Discord",
    avatarUrl = avatarUrl,
    isActive = isActive,
    isDiscordConnected = isDiscordConnected,
)

fun ProfileSettingsDto.toDomain() = UserSettings(
    languageCode = languageCode,
    theme = theme,
    hapticFeedbackEnabled = hapticFeedbackEnabled,
    soundEffectsEnabled = soundEffectsEnabled,
    telemetryEnabled = telemetryEnabled,
)

fun ProfileSettingsDto.toProfileSettingsData() = ProfileSettingsData(
    userSettings = toDomain(),
    settingsUpdatedAtUtc = updatedAtUtc,
)

fun ProfileDto.toDomain() = ProfileData(
    user = user.toDomain(),
    userSettings = userSettings.toDomain(),
    settingsUpdatedAtUtc = userSettings.updatedAtUtc,
)
