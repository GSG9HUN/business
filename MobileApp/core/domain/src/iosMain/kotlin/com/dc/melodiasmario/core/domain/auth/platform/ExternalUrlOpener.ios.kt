package com.dc.melodiasmario.core.domain.auth.platform


import platform.Foundation.NSURL
import platform.UIKit.UIApplication
import org.koin.core.annotation.Single

@Suppress("EXPECT_ACTUAL_CLASSIFIERS_ARE_IN_BETA_WARNING")
@Single
actual class ExternalUrlOpener {
    actual fun openUrl(url: String) {
        val nsUrl = NSURL.URLWithString(url) ?: return
        UIApplication.sharedApplication.openURL(nsUrl)
    }
}
