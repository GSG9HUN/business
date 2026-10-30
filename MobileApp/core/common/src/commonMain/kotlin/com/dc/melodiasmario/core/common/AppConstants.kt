package com.dc.melodiasmario.core.common

/**
 * Groups app constants values used by the application.
 */
object AppConstants {

    /**
     * Groups urls values used by app constants.
     */
    object URLs{
        /**
         * Base url exposed by urls.
         */
        const val BaseUrl: String = "http://192.168.100.84:5281/api"

    }

    /**
     * Groups properties values used by app constants.
     */
    object Properties{
        /**
         * Api base url exposed by properties.
         */
        const val ApiBaseUrl: String = "api_base_url"
    }

    /**
     * Polling interval ms exposed by app constants.
     */
    const val PollingIntervalMs: Long = 2000L
}
