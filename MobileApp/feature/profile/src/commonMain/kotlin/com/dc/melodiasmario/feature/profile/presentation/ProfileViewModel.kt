package com.dc.melodiasmario.feature.profile.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.domain.auth.usecase.DiscordLogoutUseCase
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.profile.usecase.ClearUserSettingsUseCase
import com.dc.melodiasmario.core.model.profile.ProfileData
import com.dc.melodiasmario.core.model.profile.ProfileSettingsData
import com.dc.melodiasmario.core.domain.profile.usecase.GetProfileUseCase
import com.dc.melodiasmario.core.domain.profile.usecase.SaveUserSettingsUseCase
import com.dc.melodiasmario.core.domain.profile.usecase.UpdateProfileSettingsUseCase
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class ProfileViewModel(
    private val getProfileUseCase: GetProfileUseCase,
    private val updateProfileSettingsUseCase: UpdateProfileSettingsUseCase,
    private val discordLogoutUseCase: DiscordLogoutUseCase,
    private val saveUserSettingsUseCase: SaveUserSettingsUseCase,
    private val clearUserSettingsUseCase: ClearUserSettingsUseCase
) : ViewModel() {
    private val _uiState = MutableStateFlow(ProfileUiState())
    val uiState: StateFlow<ProfileUiState> = _uiState.asStateFlow()
    private val events = MutableSharedFlow<ProfileEvent>(extraBufferCapacity = 64)
    private val _effect = MutableSharedFlow<ProfileEffect>(extraBufferCapacity = 64)
    val effect = _effect.asSharedFlow()

    init {
        collectEvents()
    }

    fun onEvent(event: ProfileEvent) {
        viewModelScope.launch {
            events.emit(event)
        }
    }

    private fun collectEvents() {
        viewModelScope.launch {
            events.collect { event ->
                handleEvent(event)
            }
        }
    }

    private suspend fun handleEvent(event: ProfileEvent) {
        when (event) {
            ProfileEvent.ScreenOpened -> onScreenOpened()
            ProfileEvent.BackClicked -> onBackClicked()
            ProfileEvent.AppearanceClicked -> onAppearanceClicked()
            ProfileEvent.LanguageClicked -> onLanguageClicked()
            ProfileEvent.LogoutClicked -> onLogoutClicked()
            ProfileEvent.RefreshClicked -> onRefreshClicked()
            is ProfileEvent.HapticsChanged -> onHapticsChanged(event.enabled)
            is ProfileEvent.SoundEffectsChanged -> onSoundEffectsChanged(event.enabled)
            is ProfileEvent.TelemetryChanged -> onTelemetryChanged(event.enabled)
            is ProfileEvent.LanguageSelected -> onLanguageSelected(event.languageCode)
            is ProfileEvent.ThemeSelected -> onThemeSelected(event.theme)
            ProfileEvent.CancelSettingsClicked -> onCancelSettingsClicked()
            ProfileEvent.SaveSettingsClicked -> onSaveSettingsClicked()
        }
    }


    private suspend fun onScreenOpened() {
        loadProfile()
    }

    private suspend fun loadProfile() {
        getProfileUseCase().collect { result ->
            when (result) {
                Resource.Loading -> onLoading()

                is Resource.Success -> onProfileSuccess(result.data)

                is Resource.Error -> onProfileError(result.error)
            }
        }
    }

    private fun onProfileError(error: Throwable) {
        _uiState.update {
            it.copy(
                isLoading = false,
                errorMessage = error.message,
            )
        }
    }

    private suspend fun onProfileSuccess(data: ProfileData) {
        saveUserSettingsUseCase(data.userSettings)
        _uiState.update {
            it.copy(
                isLoading = false,
                user = data.user,
                userSettings = data.userSettings,
                draftUserSettings = data.userSettings,
                settingsUpdatedAtUtc = data.settingsUpdatedAtUtc,
                errorMessage = null,
            )
        }
    }

    private fun onLoading() {
        _uiState.update {
            it.copy(
                isLoading = true,
                errorMessage = null,
            )
        }
    }

    private fun onLanguageSelected(languageCode: String) {
        _uiState.update {
            it.copy(
                draftUserSettings = it.draftUserSettings.copy(languageCode = languageCode),
            )
        }
    }

    private fun onThemeSelected(theme: String) {
        _uiState.update {
            it.copy(
                draftUserSettings = it.draftUserSettings.copy(theme = theme),
            )
        }
    }

    private fun onTelemetryChanged(enabled: Boolean) {
        _uiState.update {
            it.copy(
                draftUserSettings = it.draftUserSettings.copy(telemetryEnabled = enabled),
            )
        }
    }

    private fun onHapticsChanged(enabled: Boolean) {
        _uiState.update {
            it.copy(
                draftUserSettings = it.draftUserSettings.copy(hapticFeedbackEnabled = enabled),
            )
        }
    }

    private fun onSoundEffectsChanged(enabled: Boolean) {
        _uiState.update {
            it.copy(
                draftUserSettings = it.draftUserSettings.copy(soundEffectsEnabled = enabled),
            )
        }
    }

    private fun onCancelSettingsClicked() {
        _uiState.update {
            it.copy(
                draftUserSettings = it.userSettings,
                errorMessageSaving = null,
            )
        }
    }

    private suspend fun onSaveSettingsClicked() {
        updateProfileSettingsUseCase(_uiState.value.draftUserSettings).collect { result ->
            when (result) {
                Resource.Loading -> onProfileSettingsSaving()
                is Resource.Success -> onProfileSettingsSaved(result.data)
                is Resource.Error -> onProfileSettingsSaveError(result.error)
            }
        }
    }

    private fun onProfileSettingsSaving() {
        _uiState.update {
            it.copy(
                isSaving = true,
                errorMessageSaving = null,
            )
        }
    }

    private suspend fun onProfileSettingsSaved(profileSettingsData: ProfileSettingsData) {
        saveUserSettingsUseCase(profileSettingsData.userSettings)
        _uiState.update {
            it.copy(
                userSettings = profileSettingsData.userSettings,
                draftUserSettings = profileSettingsData.userSettings,
                settingsUpdatedAtUtc = profileSettingsData.settingsUpdatedAtUtc,
                isSaving = false,
                errorMessageSaving = null,
            )
        }
        _effect.emit(ProfileEffect.SettingsSaved)
    }

    private suspend fun onProfileSettingsSaveError(error: Throwable) {
        _uiState.update {
            it.copy(
                isSaving = false,
                errorMessageSaving = error.message,
            )
        }
        _effect.emit(ProfileEffect.SettingsSavedFailed)
    }

    private suspend fun onRefreshClicked() {
        loadProfile()
    }

    private suspend fun onLogoutClicked() {
        discordLogoutUseCase().collect { result ->
            when (result) {
                Resource.Loading -> onLoading()

                is Resource.Success -> onLogoutSuccess()

                is Resource.Error -> onLogoutError(result.error)
            }
        }
    }

    private suspend fun onLogoutError(error: Throwable) {
        _uiState.update {
            it.copy(
                isLoading = false,
                errorMessage = error.message,
            )
        }

        _effect.emit(ProfileEffect.LogoutFailed)
    }

    private suspend fun onLogoutSuccess() {
        clearUserSettingsUseCase()
        _effect.emit(ProfileEffect.LogoutSuccess)
        _effect.emit(ProfileEffect.NavigateToLogin)
    }

    private suspend fun onLanguageClicked() {
        _effect.emit(ProfileEffect.OpenLanguageSelector)
    }

    private suspend fun onAppearanceClicked() {
        _effect.emit(ProfileEffect.OpenAppearanceSelector)
    }

    private suspend fun onBackClicked() {
        _effect.emit(ProfileEffect.NavigateBack)
    }
}
