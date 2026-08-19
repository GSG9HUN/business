package com.dc.melodiasmario.core.auth.data.repository

import kotlin.test.Test

// TODO: Implement tests for AuthRepositoryImpl remote calls and session persistence side effects.
class AuthRepositoryImplTest {

    @Test
    fun startDiscordLoginReturnsRemoteAuthorizeUrl() {
        // TODO: Given remote returns login URL, when startDiscordLogin is called, then same domain model is returned.
    }

    @Test
    fun exchangeTicketSavesReturnedSession() {
        // TODO: Given remote exchange succeeds, when exchangeTicket is called, then session is saved to storage.
    }

    @Test
    fun refreshSessionSavesReturnedSession() {
        // TODO: Given remote refresh succeeds, when refreshSession is called, then new session is saved to storage.
    }

    @Test
    fun logoutCallsRemoteLogoutAndClearsStoredSession() {
        // TODO: Given stored session exists, when logout is called, then remote logout runs and storage is cleared.
    }

    @Test
    fun logoutDoesNotClearSessionWhenRemoteLogoutFails() {
        // TODO: Given remote logout fails, when logout is called, then storage behavior is explicitly verified.
    }
}
