package com.dc.melodiasmario.core.commonui.biometric

import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import com.dc.melodiasmario.core.presentation.biometric.AppLockController
import kotlinx.cinterop.ExperimentalForeignApi
import platform.Foundation.NSNotificationCenter
import platform.Foundation.NSOperationQueue
import platform.UIKit.UIApplication
import platform.UIKit.UIApplicationDidEnterBackgroundNotification
import platform.UIKit.UIApplicationState
import platform.UIKit.UIApplicationWillEnterForegroundNotification

/**
 * Renders bind app lock lifecycle from the supplied parameters and forwards interaction callbacks.
 */
@OptIn(ExperimentalForeignApi::class)
@Composable
actual fun BindAppLockLifecycle(
    appLockController: AppLockController
): Unit {
    DisposableEffect(key1 = appLockController) {
        val center = NSNotificationCenter.defaultCenter

        if (UIApplication.sharedApplication.applicationState == UIApplicationState.UIApplicationStateBackground) {
            appLockController.onBackground()
        } else {
            appLockController.onForeground()
        }

        val backgroundObserver = center.addObserverForName(
            name = UIApplicationDidEnterBackgroundNotification,
            `object` = null,
            queue = NSOperationQueue.mainQueue,
            usingBlock = {
                appLockController.onBackground()
            }
        )

        val foregroundObserver = center.addObserverForName(
            name = UIApplicationWillEnterForegroundNotification,
            `object` = null,
            queue = NSOperationQueue.mainQueue,
            usingBlock = {
                appLockController.onForeground()
            }
        )

        onDispose {
            center.removeObserver(backgroundObserver)
            center.removeObserver(foregroundObserver)
        }
    }
}
