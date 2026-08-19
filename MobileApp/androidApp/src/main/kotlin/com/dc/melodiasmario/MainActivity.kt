package com.dc.melodiasmario

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import com.dc.melodiasmario.core.auth.presentation.AuthDeepLinkDispatcher
import com.dc.melodiasmario.core.settings.data.UserSettingsStorage
import com.dc.melodiasmario.core.settings.data.UserSettingsStore
import org.koin.android.ext.android.getKoin
import org.koin.android.ext.android.inject

class MainActivity : ComponentActivity() {
    private val userSettingsStore: UserSettingsStore by inject()
    private val userSettingsStorage: UserSettingsStorage by inject()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        handleDeepLink(intent)

        setContent {
            MelodiasMarioRoot(
                userSettingsStore = userSettingsStore,
                userSettingsStorage = userSettingsStorage,
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
