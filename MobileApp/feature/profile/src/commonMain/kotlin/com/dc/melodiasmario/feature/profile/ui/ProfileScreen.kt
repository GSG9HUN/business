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
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.ui.theme.MmSurface
import com.dc.melodiasmario.feature.profile.domain.model.UserProfile
import com.dc.melodiasmario.feature.profile.generated.resources.Res as ProfileRes
import com.dc.melodiasmario.feature.profile.generated.resources.profile_cancel_settings_button
import com.dc.melodiasmario.feature.profile.generated.resources.profile_save_settings_button
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
        modifier = modifier.fillMaxSize(),
        containerColor = MmSurface,
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
            uiState.isLoading -> ProfileLoadingContent(modifier = Modifier.padding(innerPadding))

            uiState.errorMessage != null -> ProfileErrorContent(
                modifier = Modifier.padding(innerPadding),
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
                    isDiscordConnected = true,
                ),
                userSettings = UserSettings(
                    languageCode = "en",
                    theme = "light",
                    hapticFeedbackEnabled = true,
                    soundEffectsEnabled = true,
                    telemetryEnabled = true,
                ),
                settingsUpdatedAtUtc = "2023-01-01T00:00:00Z",
            ),
        )
    }
}
