package com.dc.melodiasmario.shared.core

sealed interface Resource<out T> {
    data class Success<T>(val data: T) : Resource<T>
    data class Error(val error: Throwable) : Resource<Nothing>
    data object Loading : Resource<Nothing>
}