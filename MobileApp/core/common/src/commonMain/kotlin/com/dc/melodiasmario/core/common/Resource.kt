package com.dc.melodiasmario.core.common

/**
 * Provides resource for the common layer.
 */
sealed interface Resource<out T> {
    /**
     * Provides success for the common layer.
     *
     * @property data Data.
     */
    data class Success<T>(val data: T) : Resource<T>

    /**
     * Provides error for the common layer.
     *
     * @property error Error.
     */
    data class Error(val error: Throwable) : Resource<Nothing>

    /**
     * Groups loading values used by resource.
     */
    data object Loading : Resource<Nothing>
}
