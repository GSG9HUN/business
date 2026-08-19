package com.dc.melodiasmario.feature.profile.data.remote.dto

import com.dc.melodiasmario.core.settings.domain.model.UserSettings
import com.dc.melodiasmario.feature.profile.domain.model.ProfileData
import com.dc.melodiasmario.feature.profile.domain.model.UserProfile
import kotlinx.serialization.Serializable

@Serializable
data class ProfileDto(
    val user: UserProfileDto,
    val userSettings: UserSettingsDto,
)

@Serializable
data class UserProfileDto(
    val discordUserId: String,
    val username: String,
    val displayName: String,
    val avatarUrl: String?,
    val isDiscordConnected: Boolean,
    val isActive: Boolean,
)

@Serializable
data class UserSettingsDto(
    val languageCode: String,
    val theme: String,
    val hapticFeedbackEnabled: Boolean,
    val soundEffectsEnabled: Boolean,
    val telemetryEnabled: Boolean,
    val updatedAtUtc: String,
)

fun UserProfileDto.toDomain() = UserProfile(
    id = discordUserId,
    displayName = displayName,
    username = username,
    provider = "Discord",
    avatarUrl = avatarUrl,
    isActive = isActive,
    isDiscordConnected = isDiscordConnected,
)

fun UserSettingsDto.toDomain() = UserSettings(
    languageCode = languageCode,
    theme = theme,
    hapticFeedbackEnabled = hapticFeedbackEnabled,
    soundEffectsEnabled = soundEffectsEnabled,
    telemetryEnabled = telemetryEnabled,
)

fun ProfileDto.toDomain() = ProfileData(
    user = user.toDomain(),
    userSettings = userSettings.toDomain(),
    settingsUpdatedAtUtc = userSettings.updatedAtUtc,
)
