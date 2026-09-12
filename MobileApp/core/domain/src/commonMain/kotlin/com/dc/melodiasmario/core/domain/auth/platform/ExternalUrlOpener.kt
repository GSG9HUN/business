package com.dc.melodiasmario.core.domain.auth.platform

expect class ExternalUrlOpener {
    fun openUrl(url: String)
}
