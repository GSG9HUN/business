package com.dc.melodiasmario.core.commonui.biometric

import android.app.Activity
import android.content.Context
import android.content.ContextWrapper
import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.ui.platform.LocalContext
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.LifecycleEventObserver
import androidx.lifecycle.compose.LocalLifecycleOwner
import com.dc.melodiasmario.core.presentation.biometric.AppLockController

@Composable
actual fun BindAppLockLifecycle(
    controller: AppLockController
) {
    val lifecycleOwner = LocalLifecycleOwner.current
    val context = LocalContext.current

    DisposableEffect(lifecycleOwner, context, controller) {
        val activity = context.findActivity()

        val observer = LifecycleEventObserver { _, event ->
            when (event) {
                Lifecycle.Event.ON_START -> {
                    controller.onForeground()
                }

                Lifecycle.Event.ON_STOP -> {
                    if (activity?.isChangingConfigurations != true) {
                        controller.onBackground()
                    }
                }

                else -> Unit
            }
        }

        lifecycleOwner.lifecycle.addObserver(observer)

        onDispose {
            lifecycleOwner.lifecycle.removeObserver(observer)
        }
    }
}

private fun Context.findActivity(): Activity? {
    var current: Context = this

    while (current is ContextWrapper) {
        if (current is Activity) {
            return current
        }

        current = current.baseContext
    }

    return current as? Activity
}