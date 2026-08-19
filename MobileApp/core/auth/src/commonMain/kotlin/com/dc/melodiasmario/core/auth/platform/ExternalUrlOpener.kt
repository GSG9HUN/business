package com.dc.melodiasmario.core.auth.platform

expect class ExternalUrlOpener {
    fun openUrl(url: String)
}