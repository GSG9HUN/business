package com.dc.melodiasmario.shared.platform

import android.content.Context
import android.content.Intent
import androidx.core.net.toUri
import org.koin.core.annotation.Single

@Suppress("EXPECT_ACTUAL_CLASSIFIERS_ARE_IN_BETA_WARNING")
@Single
actual class ExternalUrlOpener(
    private val context: Context
) {
    actual fun openUrl(url: String) {
        val intent = Intent(Intent.ACTION_VIEW, url.toUri())
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
        context.startActivity(intent)

    }
}