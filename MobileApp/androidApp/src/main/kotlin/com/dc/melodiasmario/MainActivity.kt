package com.dc.melodiasmario

import android.content.Intent
import android.os.Bundle
import androidx.activity.compose.setContent
import androidx.compose.runtime.remember
import androidx.fragment.app.FragmentActivity
import com.dc.melodiasmario.core.commonui.biometric.BiometricAuthenticator
import com.dc.melodiasmario.core.commonui.feedback.FeedbackVibrator
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.core.domain.auth.usecase.AuthDeepLinkDispatcher
import com.dc.melodiasmario.core.domain.auth.usecase.RefreshSessionUseCase
import com.dc.melodiasmario.core.datastore.settings.UserSettingsStorage
import com.dc.melodiasmario.core.data.settings.UserSettingsStore
import com.dc.melodiasmario.core.datastore.locale.AppLocaleController
import com.dc.melodiasmario.core.presentation.biometric.AppLockController
import com.dc.melodiasmario.feedback.AppVisibility
import org.koin.android.ext.android.getKoin
import org.koin.android.ext.android.inject

class MainActivity : FragmentActivity() {
    private val userSettingsStore: UserSettingsStore by inject()
    private val userSettingsStorage: UserSettingsStorage by inject()
    private val appLocaleController: AppLocaleController by inject()
    private val refreshSessionUseCase: RefreshSessionUseCase by inject()
    private val toastHostState: MToastHostState by inject()
    private val appVisibility: AppVisibility by inject()
    private val feedbackVibrator: FeedbackVibrator by inject()
    private val appLockController: AppLockController by inject()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        handleDeepLink(intent)

        setContent {
            val biometricAuthenticator = remember(this@MainActivity) {
                BiometricAuthenticator(
                    activity = this@MainActivity
                )
            }

            MelodiasMarioRoot(
                userSettingsStore = userSettingsStore,
                userSettingsStorage = userSettingsStorage,
                appLocaleController = appLocaleController,
                refreshSessionUseCase = refreshSessionUseCase,
                toastHostState = toastHostState,
                isAppForeground = { appVisibility.isForeground },
                feedbackVibrator = feedbackVibrator,
                appLockController = appLockController,
                biometricAuthenticator = biometricAuthenticator
            )
        }
    }

    override fun onNewIntent(intent: Intent) {
        super.onNewIntent(intent)
        setIntent(intent)
        handleDeepLink(intent)
    }

    private fun handleDeepLink(intent: Intent?) {
        val uri = intent?.data ?: return

        if (uri.scheme == "myapp" && uri.host == "auth") {
            val ticket = uri.getQueryParameter("ticket")
            val authDeepLinkDispatcher = getKoin().get<AuthDeepLinkDispatcher>()

            ticket?.takeIf { it.isNotBlank() }?.let {
                authDeepLinkDispatcher.dispatchTicket(ticket = it)
            }
        }
    }

}
