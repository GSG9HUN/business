package com.dc.melodiasmario.feature.profile.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.auth.usecase.DiscordLogoutUseCase
import com.dc.melodiasmario.core.domain.profile.usecase.ClearUserSettingsUseCase
import com.dc.melodiasmario.core.domain.profile.usecase.GetProfileUseCase
import com.dc.melodiasmario.core.domain.profile.usecase.SaveUserSettingsUseCase
import com.dc.melodiasmario.core.domain.profile.usecase.UpdateProfileSettingsUseCase
import com.dc.melodiasmario.core.model.profile.ProfileData
import com.dc.melodiasmario.core.model.profile.ProfileSettingsData
import com.dc.melodiasmario.core.presentation.runAction
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import org.koin.core.annotation.KoinViewModel

/**
 * Coordinates profile events, observable UI state, and one-time effects.
 */
@KoinViewModel
class ProfileViewModel(
    private val getProfileUseCase: GetProfileUseCase,
    private val updateProfileSettingsUseCase: UpdateProfileSettingsUseCase,
    private val discordLogoutUseCase: DiscordLogoutUseCase,
    private val saveUserSettingsUseCase: SaveUserSettingsUseCase,
    private val clearUserSettingsUseCase: ClearUserSettingsUseCase
) : ViewModel() {
    private val _uiState = MutableStateFlow(value = ProfileUiState())

    /**
     * Read-only presentation state observed by UI consumers.
     */
    val uiState: StateFlow<ProfileUiState> = _uiState.asStateFlow()
    private val events = MutableSharedFlow<ProfileEvent>(extraBufferCapacity = 64)
    private val _effect = MutableSharedFlow<ProfileEffect>(extraBufferCapacity = 64)

    /**
     * One-time presentation outcomes observed by UI consumers.
     */
    val effect: SharedFlow<ProfileEffect> = _effect.asSharedFlow()

    init {
        collectEvents()
    }

    /**
     * Submits a user or lifecycle event to the feature event-processing flow.
     */
    fun onEvent(event: ProfileEvent): Unit {
        viewModelScope.launch {
            events.emit(value = event)
        }
    }

    private fun collectEvents() {
        viewModelScope.launch {
            events.collect { event ->
                handleEvent(event = event)
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
            is ProfileEvent.HapticsChanged -> onHapticsChanged(enabled = event.enabled)
            is ProfileEvent.SoundEffectsChanged -> onSoundEffectsChanged(enabled = event.enabled)
            is ProfileEvent.TelemetryChanged -> onTelemetryChanged(enabled = event.enabled)
            is ProfileEvent.LanguageSelected -> onLanguageSelected(languageCode = event.languageCode)
            is ProfileEvent.ThemeSelected -> onThemeSelected(theme = event.theme)
            ProfileEvent.CancelSettingsClicked -> onCancelSettingsClicked()
            ProfileEvent.SaveSettingsClicked -> onSaveSettingsClicked()
            is ProfileEvent.BiometricLockChanged -> onBiometricLockChanged(enabled = event.enabled)

        }
    }

    private suspend fun onBiometricLockChanged(enabled: Boolean) {
        _effect.emit(
            value = ProfileEffect.ChangeBiometricLock(enabled = enabled)
        )
    }

    private suspend fun onScreenOpened() {
        loadProfile()
    }

    private suspend fun loadProfile() {
        getProfileUseCase().collect { result ->
            when (result) {
                Resource.Loading -> onLoading()

                is Resource.Success -> onProfileSuccess(data = result.data)

                is Resource.Error -> onProfileError(error = result.error)
            }
        }
    }

    private fun onProfileError(error: Throwable) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                isLoading = false,
                errorMessage = error.message
            )
        }
    }

    private suspend fun onProfileSuccess(data: ProfileData) {
        saveUserSettingsUseCase(userSettings = data.userSettings)
        _uiState.update { currentUiState ->
            currentUiState.copy(
                isLoading = false,
                user = data.user,
                userSettings = data.userSettings,
                draftUserSettings = data.userSettings,
                settingsUpdatedAtUtc = data.settingsUpdatedAtUtc,
                errorMessage = null
            )
        }
    }

    private fun onLoading() {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                isLoading = true,
                errorMessage = null
            )
        }
    }

    private fun onLanguageSelected(languageCode: String) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                draftUserSettings = currentUiState.draftUserSettings.copy(languageCode = languageCode)
            )
        }
    }

    private fun onThemeSelected(theme: String) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                draftUserSettings = currentUiState.draftUserSettings.copy(theme = theme)
            )
        }
    }

    private fun onTelemetryChanged(enabled: Boolean) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                draftUserSettings = currentUiState.draftUserSettings.copy(telemetryEnabled = enabled)
            )
        }
    }

    private fun onHapticsChanged(enabled: Boolean) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                draftUserSettings = currentUiState.draftUserSettings.copy(hapticFeedbackEnabled = enabled)
            )
        }
    }

    private fun onSoundEffectsChanged(enabled: Boolean) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                draftUserSettings = currentUiState.draftUserSettings.copy(soundEffectsEnabled = enabled)
            )
        }
    }

    private fun onCancelSettingsClicked() {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                draftUserSettings = currentUiState.userSettings,
                errorMessageSaving = null
            )
        }
    }

    private suspend fun onSaveSettingsClicked() {
        updateProfileSettingsUseCase(userSettings = _uiState.value.draftUserSettings).collect { result ->
            when (result) {
                Resource.Loading -> onProfileSettingsSaving()
                is Resource.Success -> onProfileSettingsSaved(profileSettingsData = result.data)
                is Resource.Error -> onProfileSettingsSaveError(error = result.error)
            }
        }
    }

    private fun onProfileSettingsSaving() {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                isSaving = true,
                errorMessageSaving = null
            )
        }
    }

    private suspend fun onProfileSettingsSaved(profileSettingsData: ProfileSettingsData) {
        saveUserSettingsUseCase(userSettings = profileSettingsData.userSettings)
        _uiState.update { currentUiState ->
            currentUiState.copy(
                userSettings = profileSettingsData.userSettings,
                draftUserSettings = profileSettingsData.userSettings,
                settingsUpdatedAtUtc = profileSettingsData.settingsUpdatedAtUtc,
                isSaving = false,
                errorMessageSaving = null
            )
        }
        _effect.emit(value = ProfileEffect.SettingsSaved)
    }

    private suspend fun onProfileSettingsSaveError(error: Throwable) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                isSaving = false,
                errorMessageSaving = error.message
            )
        }
        _effect.emit(value = ProfileEffect.SettingsSavedFailed)
    }

    private suspend fun onRefreshClicked() {
        loadProfile()
    }

    private suspend fun onLogoutClicked() {

        discordLogoutUseCase().runAction(
            uiState = _uiState,
            effects = _effect,
            successEffect = ProfileEffect.LogoutSuccess,
            failureEffect = ProfileEffect.LogoutFailed,
            onLoading = { currentUiState ->
                currentUiState.copy(isLoading = true, errorMessage = null)
            },
            onError = { state, error ->
                state.copy(
                    isLoading = false,
                    errorMessage = error.message
                )
            },
            afterSuccess = {
                clearUserSettingsUseCase()
                _effect.emit(value = ProfileEffect.LogoutSuccess)
                _effect.emit(value = ProfileEffect.NavigateToLogin)
            }
        )
    }

    private suspend fun onLanguageClicked() {
        _effect.emit(value = ProfileEffect.OpenLanguageSelector)
    }

    private suspend fun onAppearanceClicked() {
        _effect.emit(value = ProfileEffect.OpenAppearanceSelector)
    }

    private suspend fun onBackClicked() {
        _effect.emit(value = ProfileEffect.NavigateBack)
    }
}
