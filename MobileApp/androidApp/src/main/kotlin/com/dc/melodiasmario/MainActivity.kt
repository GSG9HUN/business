package com.dc.melodiasmario

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import com.dc.melodiasmario.core.auth.presentation.AuthDeepLinkDispatcher
import com.dc.melodiasmario.shared.ui.MelodiasMarioApp
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioTheme
import org.koin.android.ext.android.getKoin

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        handleDeepLink(intent)

        setContent {
            MelodiasMarioTheme {
                MelodiasMarioApp()
            }
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
