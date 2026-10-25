package com.dc.melodiasmario.di

import android.app.Application
import com.dc.melodiasmario.core.commonui.feedback.FeedbackVibrator
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.core.commonui.playbacknotification.PlaybackNotificationController
import com.dc.melodiasmario.core.datastore.locale.AppLocaleController
import com.dc.melodiasmario.feature.queue.presentation.playlist.QueuePlaylistSaveFeedback
import com.dc.melodiasmario.feedback.AndroidPlaylistSaveFeedback
import com.dc.melodiasmario.feedback.AppVisibility
import org.koin.android.ext.koin.androidContext
import org.koin.dsl.module

val androidAppModule = module {
    single {
        AppLocaleController(
            context = androidContext()
        )
    }
    single {
        PlaybackNotificationController(
            context = androidContext()
        )
    }
    single(createdAtStart = true) {
        AppVisibility(
            application = androidContext() as Application,
        )
    }
    single {
        MToastHostState()
    }
    single<QueuePlaylistSaveFeedback> {
        AndroidPlaylistSaveFeedback(
            context = androidContext(),
            appVisibility = get(),
            toastHostState = get(),
        )
    }
    single {
        FeedbackVibrator(
            context = androidContext(),
        )
    }
}
