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
import kotlinx.coroutines.test.TestResult
import kotlinx.coroutines.test.runTest
import kotlinx.serialization.Serializable
import kotlin.test.AfterTest
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue
import kotlin.test.Test

class HttpClientTest {

    private var client: HttpClient? = null

    @AfterTest
    fun tearDown() {
        client?.close()
        client = null
    }

    @Test
    fun createHttpClientInstallsJsonContentNegotiation(): TestResult = runTest {
        client = HttpClient(MockEngine) {
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

        val payload = requireNotNull(client).get("https://example.test/payload").body<TestPayload>()

        assertEquals("value", payload.known)
    }

    @Test
    fun createHttpClientInstallsLoggingWithAuthorizationSanitization(): TestResult = runTest {
        val logger = CapturingLogger()
        val secret = "Bearer secret-token"
        client = HttpClient(MockEngine) {
            configureMelodiasHttpClient(logger)
            engine {
                addHandler {
                    respond(content = "OK")
                }
            }
        }

        requireNotNull(client).get("https://example.test/payload") {
            header(HttpHeaders.Authorization, secret)
        }

        val logs = logger.messages.joinToString(separator = "\n")
        assertTrue(logs.contains(HttpHeaders.Authorization))
        assertFalse(logs.contains(secret))
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
