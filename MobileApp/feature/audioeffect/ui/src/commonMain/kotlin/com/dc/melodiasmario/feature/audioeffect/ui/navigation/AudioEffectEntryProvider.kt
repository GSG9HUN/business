package com.dc.melodiasmario.feature.audioeffect.ui.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.audioeffect.ui.AudioEffectsRoute

/** Registers the independent audio-effect destination and its back-stack callback. */
fun EntryProviderScope<AppRoute>.audioEffectEntry(onBack: () -> Unit, toastHostState: MToastHostState) {
    entry<AppRoute.AudioEffect> { route ->
        AudioEffectsRoute(guildId = route.guildId, onBack = onBack, toastHostState = toastHostState)
    }
}
