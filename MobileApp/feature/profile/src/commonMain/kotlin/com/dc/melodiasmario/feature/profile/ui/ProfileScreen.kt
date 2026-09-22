package com.dc.melodiasmario.feature.profile.ui

import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import com.dc.melodiasmario.core.commonui.components.MErrorScreen
import com.dc.melodiasmario.core.commonui.components.MLoadingScreen
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MFloatingSaveCancelBar
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.commonui.topbar.SetTopBarConfig
import com.dc.melodiasmario.core.commonui.topbar.TopBarConfig
import com.dc.melodiasmario.core.commonui.topbar.TopBarNavigationIcon
import com.dc.melodiasmario.core.model.profile.ProfileUser
import com.dc.melodiasmario.core.model.settings.UserSettings
import com.dc.melodiasmario.feature.profile.generated.resources.Res as ProfileRes
import com.dc.melodiasmario.feature.profile.generated.resources.profile_cancel_settings_button
import com.dc.melodiasmario.feature.profile.generated.resources.profile_back_content_description
import com.dc.melodiasmario.feature.profile.generated.resources.profile_save_settings_button
import com.dc.melodiasmario.feature.profile.generated.resources.profile_top_bar_title
import com.dc.melodiasmario.feature.profile.presentation.ProfileEvent
import com.dc.melodiasmario.feature.profile.presentation.ProfileUiState
import com.dc.melodiasmario.feature.profile.ui.components.ProfileLoadedContent
import org.jetbrains.compose.resources.stringResource

@Composable
fun ProfileScreen(
    modifier: Modifier = Modifier,
    uiState: ProfileUiState,
    onEvent: (ProfileEvent) -> Unit = {},
) {
    val colors = MelodiasMarioThemeTokens.current

    SetTopBarConfig(
        TopBarConfig(
            title = stringResource(ProfileRes.string.profile_top_bar_title),
            subTitle = uiState.user.displayName.takeIf { it.isNotBlank() },
            navigationIcon = TopBarNavigationIcon.Back(
                onClick = { onEvent(ProfileEvent.BackClicked) },
                contentDescription = stringResource(ProfileRes.string.profile_back_content_description),
            ),
        )
    )

    Scaffold(
        modifier = modifier.fillMaxSize(),
        containerColor = colors.surface,
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
            uiState.isLoading -> MLoadingScreen(modifier = Modifier.padding(innerPadding))

            uiState.errorMessage != null -> MErrorScreen(
                modifier = Modifier.padding(innerPadding),
                errorMessage = uiState.errorMessage,
                onClick = { onEvent(ProfileEvent.RefreshClicked) },
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
                user = ProfileUser(
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
