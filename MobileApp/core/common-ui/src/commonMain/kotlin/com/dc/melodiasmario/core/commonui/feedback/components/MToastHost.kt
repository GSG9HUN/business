package com.dc.melodiasmario.core.commonui.feedback.components

import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.core.tween
import androidx.compose.animation.fadeIn
import androidx.compose.animation.fadeOut
import androidx.compose.animation.slideInVertically
import androidx.compose.animation.slideOutVertically
import androidx.compose.foundation.layout.padding
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberUpdatedState
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.feedback.FeedbackVibrator
import com.dc.melodiasmario.core.commonui.feedback.model.MToastData
import com.dc.melodiasmario.core.commonui.feedback.model.MToastType
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import kotlin.time.Duration.Companion.milliseconds
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.collectLatest

/**
 * Renders toast host from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MToastHost(
    hostState: MToastHostState,
    hapticsEnabled: Boolean,
    isAppForeground: () -> Boolean,
    feedbackVibrator: FeedbackVibrator,
    modifier: Modifier = Modifier
): Unit {
    var currentToast by remember {
        mutableStateOf<MToastData?>(value = null)
    }
    var visible by remember {
        mutableStateOf(value = false)
    }

    val currentFeedbackVibrator by rememberUpdatedState(newValue = feedbackVibrator)
    val currentIsAppForeground by rememberUpdatedState(newValue = isAppForeground)
    val currentHapticsEnabled by rememberUpdatedState(newValue = hapticsEnabled)

    LaunchedEffect(key1 = hostState) {
        hostState.toasts.collectLatest { toast ->
            try {
                if (!currentIsAppForeground()) {
                    return@collectLatest
                }

                currentToast = toast
                visible = true

                val shouldVibrate = when (toast.type) {
                    MToastType.Success,
                    MToastType.Error -> true

                    MToastType.Warning,
                    MToastType.Info -> false
                }

                if (currentHapticsEnabled && shouldVibrate) {
                    currentFeedbackVibrator.vibrate()
                }

                delay(duration = toast.durationMillis.milliseconds)

                visible = false

                delay(duration = EXIT_ANIMATION_MILLIS.milliseconds)
            } finally {
                visible = false
                currentToast = null
            }
        }
    }

    AnimatedVisibility(
        modifier = modifier.padding(top = 16.dp),
        visible = visible && currentToast != null,
        enter = slideInVertically(
            animationSpec = tween(durationMillis = ENTER_ANIMATION_MILLIS),
            initialOffsetY = { contentHeight -> -contentHeight }
        ) + fadeIn(
            animationSpec = tween(durationMillis = ENTER_ANIMATION_MILLIS)
        ),
        exit = slideOutVertically(
            animationSpec = tween(durationMillis = EXIT_ANIMATION_MILLIS),
            targetOffsetY = { contentHeight -> -contentHeight }
        ) + fadeOut(
            animationSpec = tween(durationMillis = EXIT_ANIMATION_MILLIS)
        )
    ) {
        currentToast?.let { currentToast ->
            MToast(data = currentToast)
        }
    }
}

private const val ENTER_ANIMATION_MILLIS = 240
private const val EXIT_ANIMATION_MILLIS = 180
