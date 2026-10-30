package com.dc.melodiasmario.feedback

import android.app.Activity
import android.app.Application
import android.os.Bundle

/**
 * Provides app visibility for the feedback layer.
 */
class AppVisibility(
    application: Application
) : Application.ActivityLifecycleCallbacks {

    private var startedActivities = 0

    /**
     * Is foreground exposed by app visibility.
     */
    @Volatile
    var isForeground: Boolean = false
        private set

    init {
        application.registerActivityLifecycleCallbacks(this)
    }

    override fun onActivityStarted(activity: Activity): Unit {
        startedActivities++
        isForeground = true
    }

    override fun onActivityStopped(activity: Activity): Unit {
        startedActivities = (startedActivities - 1).coerceAtLeast(minimumValue = 0)
        isForeground = startedActivities > 0
    }

    override fun onActivityCreated(
        activity: Activity,
        savedInstanceState: Bundle?
    ): Unit = Unit

    override fun onActivityResumed(activity: Activity): Unit = Unit

    override fun onActivityPaused(activity: Activity): Unit = Unit

    override fun onActivitySaveInstanceState(
        activity: Activity,
        outState: Bundle
    ): Unit = Unit

    override fun onActivityDestroyed(activity: Activity): Unit = Unit
}
