package com.dc.melodiasmario.feature.profile.ui

import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.dc.melodiasmario.core.settings.domain.model.UserSettings
import com.dc.melodiasmario.core.ui.selector.components.MOptionSelectorBottomSheet
import com.dc.melodiasmario.core.ui.feedback.model.MToastData
import com.dc.melodiasmario.core.ui.feedback.model.MToastType
import com.dc.melodiasmario.core.ui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.profile.presentation.ProfileViewModel
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioTheme
import com.dc.melodiasmario.feature.profile.domain.model.ProfileUser
import com.dc.melodiasmario.feature.profile.generated.resources.Res
import com.dc.melodiasmario.feature.profile.generated.resources.profile_appearance_title
import com.dc.melodiasmario.feature.profile.generated.resources.profile_language_title
import com.dc.melodiasmario.feature.profile.generated.resources.profile_logout_failed
import com.dc.melodiasmario.feature.profile.generated.resources.profile_logout_success
import com.dc.melodiasmario.feature.profile.generated.resources.profile_save_failed
import com.dc.melodiasmario.feature.profile.generated.resources.profile_save_success
import com.dc.melodiasmario.feature.profile.presentation.ProfileEffect
import com.dc.melodiasmario.feature.profile.presentation.ProfileEvent
import com.dc.melodiasmario.feature.profile.presentation.ProfileUiState
import com.dc.melodiasmario.feature.profile.ui.selector.ProfileSelectorType
import com.dc.melodiasmario.feature.profile.ui.selector.profileLanguageOptions
import com.dc.melodiasmario.feature.profile.ui.selector.profileThemeOptions
import org.jetbrains.compose.resources.stringResource
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun ProfileRoute(
    modifier: Modifier = Modifier,
    onBack: () -> Unit = {},
    logout: () -> Unit = {},
    toastHostState: MToastHostState,
    viewModel: ProfileViewModel = koinViewModel(),
) {
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    var activeSelector by remember { mutableStateOf<ProfileSelectorType?>(null) }
    val profileSaveSuccessText = stringResource(Res.string.profile_save_success)
    val profileSaveFailedText = stringResource(Res.string.profile_save_failed)
    val logoutFailedText = stringResource(Res.string.profile_logout_failed)
    val logoutSuccessText = stringResource(Res.string.profile_logout_success)

    LaunchedEffect(viewModel) {
        viewModel.effect.collect { effect ->
            when (effect) {
                ProfileEffect.NavigateBack -> onBack()

                ProfileEffect.NavigateToLogin -> logout()

                ProfileEffect.OpenAppearanceSelector -> {
                    activeSelector = ProfileSelectorType.Theme
                }

                ProfileEffect.OpenLanguageSelector -> {
                    activeSelector = ProfileSelectorType.Language
                }

                ProfileEffect.SettingsSaved -> {
                    toastHostState.showToast(
                        MToastData(
                            message = profileSaveSuccessText,
                            type = MToastType.Success,
                        )
                    )
                }

                ProfileEffect.SettingsSavedFailed -> {
                    toastHostState.showToast(
                        MToastData(
                            message = profileSaveFailedText,
                            type = MToastType.Error,
                        )
                    )
                }

                ProfileEffect.LogoutFailed -> {
                    toastHostState.showToast(
                        MToastData(
                            message = logoutFailedText,
                            type = MToastType.Error,
                        )
                    )

                }

                ProfileEffect.LogoutSuccess -> {
                    toastHostState.showToast(
                        MToastData(
                            message = logoutSuccessText,
                            type = MToastType.Success,
                        )
                    )
                }
            }
        }
    }

    LaunchedEffect(Unit) {
        viewModel.onEvent(ProfileEvent.ScreenOpened)
    }
    ProfileScreen(
        modifier = modifier, uiState = uiState, onEvent = viewModel::onEvent
    )

    when (activeSelector) {
        ProfileSelectorType.Language -> {
            MOptionSelectorBottomSheet(
                title = stringResource(Res.string.profile_language_title),
                options = profileLanguageOptions(),
                selectedValue = uiState.draftUserSettings.languageCode,
                searchEnabled = true,
                onDismiss = { activeSelector = null },
                onOptionSelected = {
                    activeSelector = null
                    viewModel.onEvent(ProfileEvent.LanguageSelected(it))
                },
            )
        }

        ProfileSelectorType.Theme -> {
            MOptionSelectorBottomSheet(
                title = stringResource(Res.string.profile_appearance_title),
                options = profileThemeOptions(),
                selectedValue = uiState.draftUserSettings.theme,
                searchEnabled = false,
                onDismiss = { activeSelector = null },
                onOptionSelected = {
                    activeSelector = null
                    viewModel.onEvent(ProfileEvent.ThemeSelected(it))
                },
            )
        }

        null -> Unit
    }

}

@Preview(showBackground = true)
@Composable
fun ProfileRoutePreview() {
    MelodiasMarioTheme {
        ProfileScreen(
            uiState = ProfileUiState(
                user = ProfileUser(
                    id = "1",
                    displayName = "John Doe",
                    username = "johndoe",
                    avatarUrl = "",
                    isActive = true,
                    provider = "Discord",
                    isDiscordConnected = true
                ), userSettings = UserSettings(
                    languageCode = "en",
                    theme = "light",
                    hapticFeedbackEnabled = true,
                    soundEffectsEnabled = true,
                    telemetryEnabled = true,
                ), settingsUpdatedAtUtc = "2023-01-01T00:00:00Z"
            )
        )
    }
}


@Preview(showBackground = true)
@Composable
fun ProfileRoutePreview2() {
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
                    isDiscordConnected = true
                ), userSettings = UserSettings(
                    languageCode = "en",
                    theme = "light",
                    hapticFeedbackEnabled = true,
                    soundEffectsEnabled = true,
                    telemetryEnabled = true,
                ), settingsUpdatedAtUtc = "2023-01-01T00:00:00Z"
            )
        )
    }
}
