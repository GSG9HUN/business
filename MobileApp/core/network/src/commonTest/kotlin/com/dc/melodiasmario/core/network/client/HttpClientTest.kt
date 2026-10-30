package com.dc.melodiasmario.core.network.client

import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.engine.mock.MockEngine
import io.ktor.client.engine.mock.respond
import io.ktor.client.plugins.logging.Logger
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.http.ContentType
import io.ktor.http.HttpHeaders
import io.ktor.http.headersOf
import kotlin.test.AfterTest
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue
import kotlinx.coroutines.test.TestResult
import kotlinx.coroutines.test.runTest
import kotlinx.serialization.Serializable

class HttpClientTest {

    private var httpClient: HttpClient? = null

    @AfterTest
    fun tearDown(): Unit {
        httpClient?.close()
        httpClient = null
    }

    @Test
    fun createHttpClientInstallsJsonContentNegotiation(): TestResult = runTest {
        httpClient = HttpClient(engineFactory = MockEngine) {
            configureMelodiasHttpClient()
            engine {
                addHandler {
                    respond(
                        content = """{"known":"value","unknown":"ignored"}""",
                        headers = headersOf(
                            HttpHeaders.ContentType,
                            ContentType.Application.Json.toString()
                        )
                    )
                }
            }
        }

        val payload = requireNotNull(value = httpClient).get("https://example.test/payload")
            .body<TestPayload>()

        assertEquals(expected = "value", actual = payload.known)
    }

    @Test
    fun createHttpClientInstallsLoggingWithAuthorizationSanitization(): TestResult = runTest {
        val logger = CapturingLogger()
        val secret = "Bearer secret-token"
        httpClient = HttpClient(engineFactory = MockEngine) {
            configureMelodiasHttpClient(logger = logger)
            engine {
                addHandler {
                    respond(content = "OK")
                }
            }
        }

        requireNotNull(value = httpClient).get("https://example.test/payload") {
            header(key = HttpHeaders.Authorization, value = secret)
        }

        val logs = logger.messages.joinToString(separator = "\n")
        assertTrue(actual = logs.contains(other = HttpHeaders.Authorization))
        assertFalse(actual = logs.contains(other = secret))
    }

    @Serializable
    private data class TestPayload(val known: String)

    private class CapturingLogger : Logger {
        val messages = mutableListOf<String>()

        override fun log(message: String) {
            messages += message
        }
    }
}
