package com.dc.melodiasmario.feature.profile.ui

import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import com.dc.melodiasmario.core.settings.domain.model.UserSettings
import com.dc.melodiasmario.core.ui.components.button.MFloatingSaveCancelBar
import com.dc.melodiasmario.core.ui.components.display.MText
import com.dc.melodiasmario.core.ui.generated.resources.Res as CoreUiRes
import com.dc.melodiasmario.core.ui.generated.resources.ic_profile_appearance
import com.dc.melodiasmario.core.ui.generated.resources.ic_profile_appearance_light
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.ui.theme.MmSurface
import com.dc.melodiasmario.feature.profile.domain.model.UserProfile
import com.dc.melodiasmario.feature.profile.generated.resources.Res as ProfileRes
import com.dc.melodiasmario.feature.profile.generated.resources.profile_language_english
import com.dc.melodiasmario.feature.profile.generated.resources.profile_language_hungarian
import com.dc.melodiasmario.feature.profile.generated.resources.profile_cancel_settings_button
import com.dc.melodiasmario.feature.profile.generated.resources.profile_save_settings_button
import com.dc.melodiasmario.feature.profile.generated.resources.profile_theme_dark
import com.dc.melodiasmario.feature.profile.generated.resources.profile_theme_light
import com.dc.melodiasmario.feature.profile.generated.resources.profile_theme_system
import com.dc.melodiasmario.feature.profile.generated.resources.profile_token_refreshing
import com.dc.melodiasmario.feature.profile.generated.resources.profile_token_updated
import com.dc.melodiasmario.feature.profile.presentation.ProfileEvent
import com.dc.melodiasmario.feature.profile.presentation.ProfileUiState
import com.dc.melodiasmario.feature.profile.ui.components.ProfileErrorContent
import com.dc.melodiasmario.feature.profile.ui.components.ProfileLoadedContent
import com.dc.melodiasmario.feature.profile.ui.components.ProfileLoadingContent
import org.jetbrains.compose.resources.stringResource

@Composable
fun ProfileScreen(
    modifier: Modifier = Modifier,
    uiState: ProfileUiState,
    onEvent: (ProfileEvent) -> Unit = {},
) {
    Scaffold(
        modifier = modifier.fillMaxSize(), containerColor = MmSurface,
        topBar = {
            MText(text = "Random Top bar")
        },
        bottomBar = {
            if (!uiState.isLoading && uiState.errorMessage == null && uiState.hasUnsavedChanges) {
                MFloatingSaveCancelBar(
                    saveText = stringResource(ProfileRes.string.profile_save_settings_button),
                    cancelText = stringResource(ProfileRes.string.profile_cancel_settings_button),
                    enabled = !uiState.isSaving,
                    onSaveClick = { onEvent(ProfileEvent.SaveSettingsClicked) },
                    onCancelClick = { onEvent(ProfileEvent.CancelSettingsClicked) },
                )
            }
        },
    ) { innerPadding ->
        when {
            uiState.isLoading -> ProfileLoadingContent()

            uiState.errorMessage != null -> ProfileErrorContent(
                message = uiState.errorMessage,
                onRetryClick = { onEvent(ProfileEvent.RefreshClicked) },
            )


            else -> ProfileLoadedContent(
                modifier = Modifier.padding(innerPadding),
                uiState = uiState,
                onEvent = onEvent,
            )
        }
    }
}

//TODO több nyelv esetén kiszervezno ezeket.

@Composable
private fun String.toLanguageLabel(): String = when (this.lowercase()) {
    "hu" -> stringResource(ProfileRes.string.profile_language_hungarian)
    "en" -> stringResource(ProfileRes.string.profile_language_english)
    else -> uppercase()
}

@Composable
private fun String.toThemeLabel(): String = when (this.lowercase()) {
    "dark" -> stringResource(ProfileRes.string.profile_theme_dark)
    "light" -> stringResource(ProfileRes.string.profile_theme_light)
    "system" -> stringResource(ProfileRes.string.profile_theme_system)
    else -> this
}

private fun String.toThemeIcon() = when (lowercase()) {
    "light" -> CoreUiRes.drawable.ic_profile_appearance_light
    else -> CoreUiRes.drawable.ic_profile_appearance
}

@Composable
private fun String.toTokenUpdatedLabel(): String {
    return if (isBlank()) {
        stringResource(ProfileRes.string.profile_token_refreshing)
    } else {
        stringResource(ProfileRes.string.profile_token_updated)
    }
}

@Preview
@Composable
fun ProfileScreenPreview() {
    MelodiasMarioTheme {
        ProfileScreen(
            uiState = ProfileUiState(
                profile = UserProfile(
                    id = "1",
                    displayName = "John Doe",
                    username = "johndoe",
                    avatarUrl = "",
                    isActive = false,
                    provider = "Discord",
                    isDiscordConnected = true
                ),
                userSettings = UserSettings(
                    languageCode = "en",
                    theme = "light",
                    hapticFeedbackEnabled = true,
                    soundEffectsEnabled = true,
                    telemetryEnabled = true,
                ),
                settingsUpdatedAtUtc = "2023-01-01T00:00:00Z",
            )
        )
    }
}
