package com.dc.melodiasmario

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.key
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.core.commonui.biometric.AppLockHost
import com.dc.melodiasmario.core.commonui.biometric.BiometricAuthenticator
import com.dc.melodiasmario.core.commonui.components.MLoadingScreen
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.toThemeMode
import com.dc.melodiasmario.core.commonui.feedback.FeedbackVibrator
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.core.data.settings.UserSettingsStore
import com.dc.melodiasmario.core.datastore.locale.AppLocaleController
import com.dc.melodiasmario.core.datastore.settings.UserSettingsStorage
import com.dc.melodiasmario.core.domain.auth.usecase.RefreshSessionUseCase
import com.dc.melodiasmario.core.presentation.biometric.AppLockController
import com.dc.melodiasmario.shared.presentation.navigation.AppRouteBackStackSaver
import com.dc.melodiasmario.shared.ui.MelodiasMarioApp

/**
 * Renders melodias mario root from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MelodiasMarioRoot(
    userSettingsStore: UserSettingsStore,
    userSettingsStorage: UserSettingsStorage,
    appLocaleController: AppLocaleController,
    refreshSessionUseCase: RefreshSessionUseCase,
    toastHostState: MToastHostState,
    isAppForeground: () -> Boolean,
    feedbackVibrator: FeedbackVibrator,
    appLockController: AppLockController,
    biometricAuthenticator: BiometricAuthenticator
): Unit {
    val backStack = rememberSaveable(saver = AppRouteBackStackSaver) {
        mutableStateListOf(AppRoute.Login)
    }
    val settings by userSettingsStore.settings.collectAsStateWithLifecycle()
    var appliedLanguageCode by remember { mutableStateOf(value = settings.languageCode) }
    var isAuthBootstrapComplete by remember { mutableStateOf(value = false) }

    LaunchedEffect(key1 = Unit) {
        userSettingsStorage.getSettings()
            ?.let(block = userSettingsStore::setSettings)
    }

    LaunchedEffect(key1 = settings.languageCode) {
        appLocaleController.applyLocale(languageCode = settings.languageCode)
        appliedLanguageCode = settings.languageCode
    }

    key(appliedLanguageCode) {
        MelodiasMarioTheme(
            themeMode = settings.theme.toThemeMode()
        ) {
            AppLockHost(
                appLockController = appLockController,
                biometricAuthenticator = biometricAuthenticator,
                onSignedOut = {
                    backStack.clear()
                    backStack.add(element = AppRoute.Login)
                    isAuthBootstrapComplete = true
                }
            ) {
                LaunchedEffect(key1 = Unit) {
                    if (!isAuthBootstrapComplete) {
                        refreshSessionUseCase().collect { result ->
                            when (result) {
                                is Resource.Success -> {
                                    backStack.clear()
                                    backStack.add(element = AppRoute.GuildSelector)
                                    isAuthBootstrapComplete = true
                                }

                                is Resource.Error -> {
                                    backStack.clear()
                                    backStack.add(element = AppRoute.Login)
                                    isAuthBootstrapComplete = true
                                }

                                Resource.Loading -> Unit
                            }
                        }
                    }
                }

                if (isAuthBootstrapComplete) {
                    MelodiasMarioApp(
                        backStack = backStack,
                        toastHostState = toastHostState,
                        hapticsEnabled = settings.hapticFeedbackEnabled,
                        isAppForeground = isAppForeground,
                        feedbackVibrator = feedbackVibrator
                    )
                } else {
                    MLoadingScreen()
                }
            }
        }
    }
}
