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
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.ui.components.display.MAvatar
import com.dc.melodiasmario.core.ui.components.display.MBadge
import com.dc.melodiasmario.core.ui.components.display.MText
import com.dc.melodiasmario.core.ui.components.settings.MSettingRow
import com.dc.melodiasmario.core.ui.components.settings.MToggleRow
import com.dc.melodiasmario.core.ui.generated.resources.ic_profile_haptics
import com.dc.melodiasmario.core.ui.generated.resources.ic_profile_language
import com.dc.melodiasmario.core.ui.generated.resources.ic_profile_sound_effects
import com.dc.melodiasmario.core.ui.generated.resources.ic_profile_telemetry
import com.dc.melodiasmario.core.ui.generated.resources.ic_profile_token_status
import com.dc.melodiasmario.core.ui.generated.resources.Res as CoreUiRes
import com.dc.melodiasmario.core.ui.layout.MScrollableScreenContent
import com.dc.melodiasmario.core.ui.theme.MmDangerText
import com.dc.melodiasmario.core.ui.theme.MmProfileCard
import com.dc.melodiasmario.core.ui.theme.MmProfileCardOutline
import com.dc.melodiasmario.core.ui.theme.MmProfileErrorBackground
import com.dc.melodiasmario.core.ui.theme.MmProfileErrorText
import com.dc.melodiasmario.core.ui.theme.MmProfileSuccessBackground
import com.dc.melodiasmario.core.ui.theme.MmProfileSuccessText
import com.dc.melodiasmario.core.ui.theme.MmSecondaryButtonBackground
import com.dc.melodiasmario.core.ui.theme.MmSecondaryButtonOutline
import com.dc.melodiasmario.core.ui.theme.MmSurface
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

@Composable
fun ProfileLoadedContent(
    modifier: Modifier = Modifier,
    uiState: ProfileUiState,
    onEvent: (ProfileEvent) -> Unit,
) {

    val profile = uiState.profile
    val userSettings = uiState.draftUserSettings
    val displayName = profile.displayName
    val username = profile.username
    val avatarUrl = profile.avatarUrl
    val isActive = profile.isActive
    val provider = profile.provider
    val settingsUpdatedAtUtc = uiState.settingsUpdatedAtUtc
    val connectedProviderText = if (profile.isDiscordConnected) {
        stringResource(ProfileRes.string.profile_connected_provider, provider)
    } else {
        stringResource(ProfileRes.string.profile_disconnected_provider, provider)
    }
    val statusText = if (isActive) {
        stringResource(ProfileRes.string.profile_status_active)
    } else {
        stringResource(ProfileRes.string.profile_status_inactive)
    }


    MScrollableScreenContent(
        modifier = modifier
    ) {
        Surface(
            modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp),
            shape = RoundedCornerShape(12.dp),
            color = MmProfileCard,
            border = BorderStroke(1.dp, MmProfileCardOutline),
        ) {
            Row(
                modifier = Modifier.fillMaxWidth().padding(10.dp),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                MAvatar(
                    name = displayName,
                    imageUrl = avatarUrl,
                    size = 50.dp,
                )

                Column(
                    modifier = Modifier.weight(1f)
                ) {
                    MText(
                        text = displayName,
                        textAlign = TextAlign.Start,
                        fontWeight = FontWeight.Bold
                    )
                    MText(
                        text = "@$username • $connectedProviderText", textAlign = TextAlign.Start
                    )
                }

                MBadge(
                    text = statusText,
                    backgroundColor = if (isActive) MmProfileSuccessBackground else MmProfileErrorBackground,
                    contentColor = if (isActive) MmProfileSuccessText else MmProfileErrorText,
                )
            }
        }

        Surface(
            modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp), color = MmSurface
        ) {
            Column {
                MText(
                    modifier = Modifier.padding(top = 10.dp),
                    text = stringResource(ProfileRes.string.profile_application_section),
                    fontSize = 20.sp,
                    fontWeight = FontWeight.Bold,
                )
                MSettingRow(
                    icon = CoreUiRes.drawable.ic_profile_language,
                    title = stringResource(ProfileRes.string.profile_language_title),
                    subtitle = userSettings.languageCode.toLanguageLabel(),
                    onClick = { onEvent(ProfileEvent.LanguageClicked) },
                )

                MSettingRow(
                    icon = userSettings.theme.toThemeIcon(),
                    title = stringResource(ProfileRes.string.profile_appearance_title),
                    subtitle = userSettings.theme.toThemeLabel(),
                    onClick = { onEvent(ProfileEvent.AppearanceClicked) },
                )

                MToggleRow(
                    icon = CoreUiRes.drawable.ic_profile_haptics,
                    title = stringResource(ProfileRes.string.profile_haptics_title),
                    subtitle = stringResource(ProfileRes.string.profile_haptics_subtitle),
                    checked = userSettings.hapticFeedbackEnabled,
                    onCheckedChange = { onEvent(ProfileEvent.HapticsChanged(it)) },
                )

                MToggleRow(
                    icon = CoreUiRes.drawable.ic_profile_sound_effects,
                    title = stringResource(ProfileRes.string.profile_sound_effects_title),
                    subtitle = stringResource(ProfileRes.string.profile_sound_effects_subtitle),
                    checked = userSettings.soundEffectsEnabled,
                    onCheckedChange = { onEvent(ProfileEvent.SoundEffectsChanged(it)) },
                )

                MToggleRow(
                    icon = CoreUiRes.drawable.ic_profile_telemetry,
                    title = stringResource(ProfileRes.string.profile_telemetry_title),
                    subtitle = stringResource(ProfileRes.string.profile_telemetry_subtitle),
                    checked = userSettings.telemetryEnabled,
                    onCheckedChange = { onEvent(ProfileEvent.TelemetryChanged(it)) },
                )

                MText(
                    modifier = Modifier.padding(top = 26.dp),
                    text = stringResource(ProfileRes.string.profile_session_section),
                    fontSize = 20.sp,
                    fontWeight = FontWeight.Bold,
                )

                MSettingRow(
                    icon = CoreUiRes.drawable.ic_profile_token_status,
                    title = stringResource(ProfileRes.string.profile_token_status_title),
                    subtitle = settingsUpdatedAtUtc.toTokenUpdatedLabel(),
                    trailingContent = {
                        MBadge(
                            text = stringResource(ProfileRes.string.profile_token_status_ok),
                            backgroundColor = MmProfileSuccessBackground,
                            contentColor = MmProfileSuccessText,
                        )
                    },
                )

                Button(
                    modifier = Modifier.fillMaxWidth().padding(vertical = 16.dp),
                    onClick = { onEvent(ProfileEvent.LogoutClicked) },
                    shape = RoundedCornerShape(12.dp),
                    border = BorderStroke(1.dp, MmSecondaryButtonOutline),
                    colors = ButtonDefaults.buttonColors(
                        containerColor = MmSecondaryButtonBackground,
                        contentColor = MmDangerText,
                    ),
                ) {
                    MText(
                        text = stringResource(ProfileRes.string.profile_logout_button),
                        color = MmDangerText,
                        fontWeight = FontWeight.Bold,
                    )
                }
            }
        }
    }
}
