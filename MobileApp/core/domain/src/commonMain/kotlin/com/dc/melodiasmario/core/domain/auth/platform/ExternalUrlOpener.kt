package com.dc.melodiasmario.core.domain.auth.platform

/**
 * Provides external url opener for the platform layer.
 */
expect class ExternalUrlOpener {
    /**
     * Opens the supplied external URL through the platform adapter.
     */
    fun openUrl(url: String): Unit
}
