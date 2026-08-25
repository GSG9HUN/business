package com.dc.melodiasmario.core.auth.domain.usecase

import kotlin.test.Test

// TODO: Implement tests for RefreshSessionUseCase Resource flow emissions.
class RefreshSessionUseCaseTest {

    @Test
    fun invokeEmitsLoadingThenSuccessWhenRefreshTokenExists() {
        // TODO: Given stored session with refresh token, when invoked, then emits Loading followed by Success.
    }

    @Test
    fun invokeCallsRepositoryWithStoredRefreshToken() {
        // TODO: Given stored session, when invoked, then repository receives the same refresh token.
    }

    @Test
    fun invokeEmitsLoadingThenErrorWhenNoSessionExists() {
        // TODO: Given empty storage, when invoked, then emits Loading followed by Error.
    }

    @Test
    fun invokeEmitsLoadingThenErrorWhenRepositoryRefreshFails() {
        // TODO: Given repository refresh throws, when invoked, then emits Loading followed by Error.
    }
}
