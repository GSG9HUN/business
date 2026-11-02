package com.dc.melodiasmario.shared.ui

import androidx.compose.runtime.Composable
import androidx.compose.runtime.snapshots.SnapshotStateList
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.core.commonui.feedback.FeedbackVibrator
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.shared.presentation.navigation.Navigation

/**
 * Renders melodias mario app from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MelodiasMarioApp(
    backStack: SnapshotStateList<AppRoute>,
    toastHostState: MToastHostState,
    hapticsEnabled: Boolean,
    isAppForeground: () -> Boolean,
    feedbackVibrator: FeedbackVibrator
): Unit {
    Navigation(
        backStack = backStack,
        toastHostState = toastHostState,
        hapticsEnabled = hapticsEnabled,
        isAppForeground = isAppForeground,
        feedbackVibrator = feedbackVibrator
    )
}
