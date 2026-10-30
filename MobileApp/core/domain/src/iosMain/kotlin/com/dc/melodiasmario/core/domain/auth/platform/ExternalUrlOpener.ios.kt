package com.dc.melodiasmario.core.domain.auth.platform

import org.koin.core.annotation.Single
import platform.Foundation.NSURL
import platform.UIKit.UIApplication

/**
 * Provides external url opener for the platform layer.
 */
@Suppress("EXPECT_ACTUAL_CLASSIFIERS_ARE_IN_BETA_WARNING")
@Single
actual class ExternalUrlOpener {
    /**
     * Opens the supplied external URL through the platform adapter.
     */
    actual fun openUrl(url: String): Unit {
        val nsUrl = NSURL.URLWithString(url) ?: return
        UIApplication.sharedApplication.openURL(nsUrl)
    }
}
