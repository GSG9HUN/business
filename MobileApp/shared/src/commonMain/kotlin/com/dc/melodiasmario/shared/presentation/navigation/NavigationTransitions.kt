package com.dc.melodiasmario.shared.presentation.navigation

import androidx.compose.animation.ContentTransform
import androidx.compose.animation.core.FastOutSlowInEasing
import androidx.compose.animation.core.tween
import androidx.compose.animation.fadeIn
import androidx.compose.animation.fadeOut
import androidx.compose.animation.slideInHorizontally
import androidx.compose.animation.slideOutHorizontally
import androidx.compose.animation.togetherWith

private const val NavigationAnimationDurationMillis = 260

fun melodiasForwardTransition(): ContentTransform {
    return slideInHorizontally(
        animationSpec = tween(
            durationMillis = NavigationAnimationDurationMillis,
            easing = FastOutSlowInEasing,
        ),
        initialOffsetX = { fullWidth -> fullWidth / 4 },
    ) + fadeIn(
        animationSpec = tween(
            durationMillis = NavigationAnimationDurationMillis,
        ),
    ) togetherWith slideOutHorizontally(
        animationSpec = tween(
            durationMillis = NavigationAnimationDurationMillis,
            easing = FastOutSlowInEasing,
        ),
        targetOffsetX = { fullWidth -> -fullWidth / 4 },
    ) + fadeOut(
        animationSpec = tween(
            durationMillis = NavigationAnimationDurationMillis,
        ),
    )
}

fun melodiasPopTransition(): ContentTransform {
    return slideInHorizontally(
        animationSpec = tween(
            durationMillis = NavigationAnimationDurationMillis,
            easing = FastOutSlowInEasing,
        ),
        initialOffsetX = { fullWidth -> -fullWidth / 4 },
    ) + fadeIn(
        animationSpec = tween(
            durationMillis = NavigationAnimationDurationMillis,
        ),
    ) togetherWith slideOutHorizontally(
        animationSpec = tween(
            durationMillis = NavigationAnimationDurationMillis,
            easing = FastOutSlowInEasing,
        ),
        targetOffsetX = { fullWidth -> fullWidth / 4 },
    ) + fadeOut(
        animationSpec = tween(
            durationMillis = NavigationAnimationDurationMillis,
        ),
    )
}