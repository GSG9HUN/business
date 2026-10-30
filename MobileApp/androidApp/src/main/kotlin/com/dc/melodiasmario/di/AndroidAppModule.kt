package com.dc.melodiasmario.di

import android.app.Application
import android.os.SystemClock
import com.dc.melodiasmario.core.commonui.feedback.FeedbackVibrator
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.core.commonui.playbacknotification.PlaybackNotificationController
import com.dc.melodiasmario.core.data.auth.AuthSessionMemoryCache
import com.dc.melodiasmario.core.datastore.biometric.BiometricSettingsStorage
import com.dc.melodiasmario.core.datastore.locale.AppLocaleController
import com.dc.melodiasmario.core.domain.auth.SecureAuthSessionStorage
import com.dc.melodiasmario.core.presentation.biometric.AppLockController
import com.dc.melodiasmario.feature.queue.presentation.playlist.QueuePlaylistSaveFeedback
import com.dc.melodiasmario.feedback.AndroidPlaylistSaveFeedback
import com.dc.melodiasmario.feedback.AppVisibility
import org.koin.android.ext.koin.androidContext
import org.koin.dsl.module

/**
 * Android app module exposed by this module.
 */
val androidAppModule: org.koin.core.module.Module = module {
    single {
        AppLocaleController(
            context = androidContext()
        )
    }
    single {
        PlaybackNotificationController(
            context = androidContext()
        )
    }
    single(createdAtStart = true) {
        AppVisibility(
            application = androidContext() as Application
        )
    }
    single {
        MToastHostState()
    }
    single<QueuePlaylistSaveFeedback> {
        AndroidPlaylistSaveFeedback(
            context = androidContext(),
            appVisibility = get(),
            toastHostState = get()
        )
    }
    single {
        FeedbackVibrator(
            context = androidContext()
        )
    }
    single {
        BiometricSettingsStorage(
            context = androidContext()
        )
    }

    single {
        val settingsStorage = get<BiometricSettingsStorage>()
        val sessionStorage = get<SecureAuthSessionStorage>()
        val sessionCache = get<AuthSessionMemoryCache>()

        AppLockController(
            hasStoredSession = {
                sessionStorage.getSession() != null
            },
            loadEnabled = {
                settingsStorage.isEnabled()
            },
            saveEnabled = { enabled ->
                settingsStorage.setEnabled(enabled = enabled)
            },
            clearLocalSession = {
                sessionStorage.clearSession()
                sessionCache.clear()
            },
            nowMillis = {
                SystemClock.elapsedRealtime()
            }
        )
    }
}
