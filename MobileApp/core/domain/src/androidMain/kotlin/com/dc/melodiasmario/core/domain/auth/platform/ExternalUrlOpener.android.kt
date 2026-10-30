package com.dc.melodiasmario.core.domain.auth.platform

import android.content.Context
import android.content.Intent
import android.net.Uri
import org.koin.core.annotation.Single

/**
 * Provides external url opener for the platform layer.
 */
@Suppress("EXPECT_ACTUAL_CLASSIFIERS_ARE_IN_BETA_WARNING")
@Single
actual class ExternalUrlOpener(
    private val context: Context
) {
    /**
     * Opens the supplied external URL through the platform adapter.
     */
    actual fun openUrl(url: String): Unit {
        val intent = Intent(Intent.ACTION_VIEW, Uri.parse(url))
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
        context.startActivity(intent)
    }
}
