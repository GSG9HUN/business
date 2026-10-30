package com.dc.melodiasmario.feature.profile.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.biometric.BiometricLockSetting
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MAvatar
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MBadge
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.components.settings.MSettingRow
import com.dc.melodiasmario.core.commonui.designsystem.components.settings.MToggleRow
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res as CoreUiRes
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_profile_haptics
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_profile_language
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_profile_sound_effects
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_profile_telemetry
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_profile_token_status
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MmBackgroundPreviewColor
import com.dc.melodiasmario.core.commonui.layout.MScrollableScreenContent
import com.dc.melodiasmario.core.model.profile.ProfileUser
import com.dc.melodiasmario.core.model.settings.UserSettings
import com.dc.melodiasmario.core.presentation.biometric.AppLockUiState
import com.dc.melodiasmario.feature.profile.generated.resources.Res as ProfileRes
import com.dc.melodiasmario.feature.profile.generated.resources.profile_appearance_title
import com.dc.melodiasmario.feature.profile.generated.resources.profile_application_section
import com.dc.melodiasmario.feature.profile.generated.resources.profile_connected_provider
import com.dc.melodiasmario.feature.profile.generated.resources.profile_disconnected_provider
import com.dc.melodiasmario.feature.profile.generated.resources.profile_haptics_subtitle
import com.dc.melodiasmario.feature.profile.generated.resources.profile_haptics_title
import com.dc.melodiasmario.feature.profile.generated.resources.profile_language_title
import com.dc.melodiasmario.feature.profile.generated.resources.profile_logout_button
import com.dc.melodiasmario.feature.profile.generated.resources.profile_session_section
import com.dc.melodiasmario.feature.profile.generated.resources.profile_sound_effects_subtitle
import com.dc.melodiasmario.feature.profile.generated.resources.profile_sound_effects_title
import com.dc.melodiasmario.feature.profile.generated.resources.profile_status_active
import com.dc.melodiasmario.feature.profile.generated.resources.profile_status_inactive
import com.dc.melodiasmario.feature.profile.generated.resources.profile_telemetry_subtitle
import com.dc.melodiasmario.feature.profile.generated.resources.profile_telemetry_title
import com.dc.melodiasmario.feature.profile.generated.resources.profile_token_status_ok
import com.dc.melodiasmario.feature.profile.generated.resources.profile_token_status_title
import com.dc.melodiasmario.feature.profile.presentation.ProfileEvent
import com.dc.melodiasmario.feature.profile.presentation.ProfileUiState
import com.dc.melodiasmario.feature.profile.ui.mapper.toLanguageLabel
import com.dc.melodiasmario.feature.profile.ui.mapper.toThemeIcon
import com.dc.melodiasmario.feature.profile.ui.mapper.toThemeLabel
import com.dc.melodiasmario.feature.profile.ui.mapper.toTokenUpdatedLabel
import org.jetbrains.compose.resources.stringResource

/**
 * Renders profile loaded content from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun ProfileLoadedContent(
    modifier: Modifier = Modifier,
    uiState: ProfileUiState,
    onEvent: (profileEvent: ProfileEvent) -> Unit,
    appLockState: AppLockUiState = AppLockUiState()
): Unit {
    val user = uiState.user
    val userSettings = uiState.draftUserSettings
    val displayName = user.displayName
    val username = user.username
    val avatarUrl = user.avatarUrl
    val isActive = user.isActive
    val provider = user.provider
    val settingsUpdatedAtUtc = uiState.settingsUpdatedAtUtc
    val connectedProviderText = if (user.isDiscordConnected) {
        stringResource(resource = ProfileRes.string.profile_connected_provider, formatArgs = arrayOf<Any>(provider))
    } else {
        stringResource(resource = ProfileRes.string.profile_disconnected_provider, formatArgs = arrayOf<Any>(provider))
    }
    val statusText = if (isActive) {
        stringResource(resource = ProfileRes.string.profile_status_active)
    } else {
        stringResource(resource = ProfileRes.string.profile_status_inactive)
    }

    MScrollableScreenContent(
        modifier = modifier
    ) {
        Surface(
            modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp),
            shape = RoundedCornerShape(size = 12.dp),
            color = MaterialTheme.colorScheme.surfaceContainer,
            border = BorderStroke(width = 1.dp, color = MaterialTheme.colorScheme.outlineVariant)
        ) {
            Row(
                modifier = Modifier.fillMaxWidth()
                    .padding(all = 10.dp),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(space = 12.dp)
            ) {
                MAvatar(
                    name = displayName,
                    imageUrl = avatarUrl,
                    size = 50.dp
                )

                Column(
                    modifier = Modifier.weight(weight = 1f)
                ) {
                    MText(
                        text = displayName,
                        textAlign = TextAlign.Start,
                        fontWeight = FontWeight.Bold
                    )
                    MText(
                        text = "@$username â€˘ $connectedProviderText",
                        textAlign = TextAlign.Start
                    )
                }

                MBadge(
                    text = statusText,
                    backgroundColor = if (isActive) MaterialTheme.colorScheme.tertiaryContainer else MaterialTheme.colorScheme.errorContainer,
                    contentColor = if (isActive) MaterialTheme.colorScheme.onTertiaryContainer else MaterialTheme.colorScheme.onErrorContainer
                )
            }
        }

        Surface(
            modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp),
            color = MaterialTheme.colorScheme.surface
        ) {
            Column {
                MText(
                    modifier = Modifier.padding(top = 10.dp),
                    text = stringResource(resource = ProfileRes.string.profile_application_section),
                    fontSize = 20.sp,
                    fontWeight = FontWeight.Bold
                )
                MSettingRow(
                    icon = CoreUiRes.drawable.ic_profile_language,
                    title = stringResource(resource = ProfileRes.string.profile_language_title),
                    subtitle = userSettings.languageCode.toLanguageLabel(),
                    onClick = { onEvent(ProfileEvent.LanguageClicked) }
                )

                MSettingRow(
                    icon = userSettings.theme.toThemeIcon(),
                    title = stringResource(resource = ProfileRes.string.profile_appearance_title),
                    subtitle = userSettings.theme.toThemeLabel(),
                    onClick = { onEvent(ProfileEvent.AppearanceClicked) }
                )

                MToggleRow(
                    icon = CoreUiRes.drawable.ic_profile_haptics,
                    title = stringResource(resource = ProfileRes.string.profile_haptics_title),
                    subtitle = stringResource(resource = ProfileRes.string.profile_haptics_subtitle),
                    checked = userSettings.hapticFeedbackEnabled,
                    onCheckedChange = { enabled -> onEvent(ProfileEvent.HapticsChanged(enabled = enabled)) }
                )

                MToggleRow(
                    icon = CoreUiRes.drawable.ic_profile_sound_effects,
                    title = stringResource(resource = ProfileRes.string.profile_sound_effects_title),
                    subtitle = stringResource(resource = ProfileRes.string.profile_sound_effects_subtitle),
                    checked = userSettings.soundEffectsEnabled,
                    onCheckedChange = { enabled -> onEvent(ProfileEvent.SoundEffectsChanged(enabled = enabled)) }
                )

                MToggleRow(
                    icon = CoreUiRes.drawable.ic_profile_telemetry,
                    title = stringResource(resource = ProfileRes.string.profile_telemetry_title),
                    subtitle = stringResource(resource = ProfileRes.string.profile_telemetry_subtitle),
                    checked = userSettings.telemetryEnabled,
                    onCheckedChange = { enabled -> onEvent(ProfileEvent.TelemetryChanged(enabled = enabled)) }
                )

                BiometricLockSetting(
                    uiState = appLockState,
                    onCheckedChange = { enabled ->
                        onEvent(ProfileEvent.BiometricLockChanged(enabled = enabled))
                    }
                )

                MText(
                    modifier = Modifier.padding(top = 26.dp),
                    text = stringResource(resource = ProfileRes.string.profile_session_section),
                    fontSize = 20.sp,
                    fontWeight = FontWeight.Bold
                )

                MSettingRow(
                    icon = CoreUiRes.drawable.ic_profile_token_status,
                    title = stringResource(resource = ProfileRes.string.profile_token_status_title),
                    subtitle = settingsUpdatedAtUtc.toTokenUpdatedLabel(),
                    trailingContent = {
                        MBadge(
                            text = stringResource(resource = ProfileRes.string.profile_token_status_ok),
                            backgroundColor = MaterialTheme.colorScheme.tertiaryContainer,
                            contentColor = MaterialTheme.colorScheme.onTertiaryContainer
                        )
                    }
                )

                Button(
                    modifier = Modifier.fillMaxWidth()
                        .padding(vertical = 16.dp),
                    onClick = { onEvent(ProfileEvent.LogoutClicked) },
                    shape = RoundedCornerShape(size = 12.dp),
                    border = BorderStroke(width = 1.dp, color = MaterialTheme.colorScheme.outline),
                    colors = ButtonDefaults.buttonColors(
                        containerColor = MaterialTheme.colorScheme.surfaceContainerHigh,
                        contentColor = MaterialTheme.colorScheme.error
                    )
                ) {
                    MText(
                        text = stringResource(resource = ProfileRes.string.profile_logout_button),
                        color = MaterialTheme.colorScheme.error,
                        fontWeight = FontWeight.Bold
                    )
                }
            }
        }
    }
}

@Preview(showBackground = true, backgroundColor = MmBackgroundPreviewColor)
@Composable
private fun ProfileLoadedContentPreview() {
    MelodiasMarioTheme {
        ProfileLoadedContent(
            uiState = ProfileUiState(
                user = ProfileUser(
                    id = "1",
                    displayName = "Mario Beats",
                    username = "mario",
                    provider = "Discord",
                    avatarUrl = null,
                    isActive = true,
                    isDiscordConnected = true
                ),
                userSettings = UserSettings.Default,
                draftUserSettings = UserSettings.Default.copy(
                    hapticFeedbackEnabled = true,
                    telemetryEnabled = true
                ),
                settingsUpdatedAtUtc = "2026-10-09T12:00:00Z"
            ),
            onEvent = {},
            appLockState = AppLockUiState(
                isInitialized = true,
                isEnabled = true,
                isLocked = false
            )
        )
    }
}
