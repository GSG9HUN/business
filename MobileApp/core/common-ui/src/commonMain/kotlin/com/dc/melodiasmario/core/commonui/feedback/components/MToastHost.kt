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
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.feedback.model.MToastData
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import kotlinx.coroutines.delay
import kotlin.time.Duration.Companion.milliseconds

@Composable
fun MToastHost(
    modifier: Modifier = Modifier,
    hostState: MToastHostState,
) {
    var currentToast by remember { mutableStateOf<MToastData?>(null) }
    var visible by remember { mutableStateOf(false) }

    LaunchedEffect(hostState) {
        hostState.toasts.collect { toast ->
            currentToast = toast
            visible = true
            delay(toast.durationMillis.milliseconds)
            visible = false
            delay(EXIT_ANIMATION_MILLIS.milliseconds)
            currentToast = null
        }
    }

    AnimatedVisibility(
        modifier = modifier.padding(top = 16.dp),
        visible = visible && currentToast != null,
        enter = slideInVertically(
            animationSpec = tween(durationMillis = ENTER_ANIMATION_MILLIS),
            initialOffsetY = { -it },
        ) + fadeIn(animationSpec = tween(durationMillis = ENTER_ANIMATION_MILLIS)),
        exit = slideOutVertically(
            animationSpec = tween(durationMillis = EXIT_ANIMATION_MILLIS),
            targetOffsetY = { -it },
        ) + fadeOut(animationSpec = tween(durationMillis = EXIT_ANIMATION_MILLIS)),
    ) {
        currentToast?.let {
            MToast(data = it)
        }
    }

}

private const val ENTER_ANIMATION_MILLIS = 240
private const val EXIT_ANIMATION_MILLIS = 180
