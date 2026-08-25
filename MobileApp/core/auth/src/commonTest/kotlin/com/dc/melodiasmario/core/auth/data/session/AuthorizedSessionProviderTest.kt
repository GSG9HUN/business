package com.dc.melodiasmario.core.auth.data.session

import kotlin.test.Test

// TODO: Implement tests for AuthorizedSessionProvider token selection and refresh behavior.
class AuthorizedSessionProviderTest {

    @Test
    fun getValidSessionThrowsWhenNoSessionExists() {
        // TODO: Given empty storage, when getValidSession is called, then it throws "No session found".
    }

    @Test
    fun getValidSessionReturnsCurrentAccessTokenWhenSessionIsStillValid() {
        // TODO: Given a valid session, when getValidSession is called, then refresh is not requested.
    }

    @Test
    fun getValidSessionRefreshesSessionWhenCurrentSessionIsExpiredOrCloseToExpiry() {
        // TODO: Given an expired session, when getValidSession is called, then repository refreshes it.
    }

    @Test
    fun getValidSessionReturnsRefreshedAccessTokenAfterRefresh() {
        // TODO: Given refresh succeeds, when getValidSession is called, then it returns the new access token.
    }
}
